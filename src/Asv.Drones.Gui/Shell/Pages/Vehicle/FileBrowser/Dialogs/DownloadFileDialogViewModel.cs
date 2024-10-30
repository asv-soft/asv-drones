using System;
using System.Buffers;
using System.Composition;
using System.IO;
using System.Reactive.Linq;
using System.Threading;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Asv.Mavlink;
using DynamicData.Binding;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;

namespace Asv.Drones.Gui;

public class DownloadFileDialogViewModel : ViewModelBaseWithValidation
{
    private readonly ILogService _log;
    private readonly FtpClientEx _ftpClientEx;
    private readonly FileSystemItemViewModel _remote;
    private readonly string _path;
    private readonly MemoryStream _stream = new();
    private readonly CancellationTokenSource _cts = new();

    public DownloadFileDialogViewModel()
        : base(WellKnownUri.UndefinedUri)
    {
        DesignTime.ThrowIfNotDesignMode();
        
        this.ValidationRule(x => x.BurstDownloadSize,
            x => uint.TryParse(x, out _), 
            RS.DownloadFileDialogViewModel_BurstDownloadSize_ValidationError);
        
        this.WhenValueChanged(x => x.IsBurstDownloadSelected)
            .Select(x => IsSizeEnteringEnabled = x)
            .Subscribe()
            .DisposeItWith(Disposable);
    }

    [ImportingConstructor]
    public DownloadFileDialogViewModel(
        ILogService log,
        FtpClientEx ftpClientEx,
        string localRootPath,
        FileSystemItemViewModel remote,
        FileSystemItemViewModel? local
    )
        : base($"{WellKnownUri.ShellPageVehicleFileBrowser}.download-file")
    {
        _log = log;
        _ftpClientEx = ftpClientEx;
        _remote = remote;

        _path = localRootPath;
        if (local != null)
        {
            _path = Path.Combine(
                local.IsDirectory ? local.Path : local.Path[..local.Path.LastIndexOf('\\')],
                _remote.Name
            );
        }

        this.ValidationRule(x => x.BurstDownloadSize,
            x =>
            {
                var a = byte.TryParse(x, out var value);
                var b = value <= 239;
                return a && b;
            }, 
            RS.DownloadFileDialogViewModel_BurstDownloadSize_ValidationError);
        
        this.WhenValueChanged(x => x.IsBurstDownloadSelected)
            .Select(x => IsSizeEnteringEnabled = x)
            .Subscribe()
            .DisposeItWith(Disposable);
    }

    [Reactive] public double DownloadProgress { get; set; }
    [Reactive] public bool IsDownloadSelected { get; set; } = true;
    [Reactive] public bool IsBurstDownloadSelected { get; set; }
    [Reactive] public string BurstDownloadSize { get; set; } = "239";
    [Reactive] public bool IsSizeEnteringEnabled { get; set; }

    public void ApplyDialog(ContentDialog dialog)
    {
        dialog.Closing += OnDialogClosing;
        
        this.IsValid()
            .Subscribe(isValid => dialog.IsPrimaryButtonEnabled = isValid)
            .DisposeItWith(Disposable);
    }

    private async void OnDialogClosing(ContentDialog sender, ContentDialogClosingEventArgs args)
    {
        if (args.Result is not ContentDialogResult.Primary)
        {
            await _cts.CancelAsync();
            return;
        }
        
        var progress = new Progress<double>();

        progress.ProgressChanged += async (_, value) =>
        {
            DownloadProgress = value;
            if (value >= 1) await _cts.CancelAsync();
        };
        args.Cancel = true;
        sender.IsPrimaryButtonEnabled = false;
        IsSizeEnteringEnabled = false;
        try
        {
            if (IsDownloadSelected)
                await _ftpClientEx.DownloadFile(_remote.Path, _stream, progress, _cts.Token);
            if (IsBurstDownloadSelected)
                await _ftpClientEx.BurstDownloadFile(_remote.Path, _stream, progress, byte.Parse(BurstDownloadSize), _cts.Token);
            await File.WriteAllBytesAsync(_path, _stream.ToArray());
            _log.Info(
                nameof(VehicleFileBrowserViewModel),
                $"File downloaded successfully: {_remote.Name}"
            );
        }
        catch (OperationCanceledException)
        {
            await File.WriteAllBytesAsync(_path, _stream.ToArray());
            _log.Warning(
                nameof(VehicleFileBrowserViewModel),
                $"File downloading was canceled: {_remote.Name}"
            );
        }
        finally
        {
            sender.Hide();
        }
    }
}
