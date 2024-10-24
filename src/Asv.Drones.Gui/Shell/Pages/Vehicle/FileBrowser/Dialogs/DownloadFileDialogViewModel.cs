using System;
using System.Buffers;
using System.Composition;
using System.IO;
using System.Threading;
using Asv.Drones.Gui.Api;
using Asv.Mavlink;
using FluentAvalonia.UI.Controls;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui;

public class DownloadFileDialogViewModel : ViewModelBase
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
    }

    [Reactive] public double DownloadProgress { get; set; }

    public void ApplyDialog(ContentDialog dialog)
    {
        dialog.Opened += OnDialogOpened;
        dialog.Closed += OnDialogClosed;
    }

    private async void OnDialogClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
    {
        await _cts.CancelAsync();
        if (args.Result != ContentDialogResult.Primary)
            return;
        if (_stream.Length > 0)
            await File.WriteAllBytesAsync(_path, _stream.ToArray());
    }

    private async void OnDialogOpened(ContentDialog sender, EventArgs args)
    {
        var progress = new Progress<double>();

        progress.ProgressChanged += (_, value) =>
        {
            DownloadProgress = value;
        };

        try
        {
            await _ftpClientEx.DownloadFile(_remote.Path, _stream, progress, _cts.Token);
            await File.WriteAllBytesAsync(_path, _stream.GetBuffer());
            _log.Info(
                nameof(VehicleFileBrowserViewModel),
                $"File downloaded successfully: {_remote.Name}"
            );
        }
        catch (OperationCanceledException)
        {
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
