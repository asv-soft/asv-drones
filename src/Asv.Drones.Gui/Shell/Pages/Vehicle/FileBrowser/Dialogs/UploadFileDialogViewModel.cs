using System;
using System.Composition;
using System.IO;
using System.Threading;
using Asv.Drones.Gui.Api;
using Asv.Mavlink;
using FluentAvalonia.UI.Controls;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui;

public class UploadFileDialogViewModel : ViewModelBase
{
    private readonly ILogService _log;
    private readonly FtpClientEx _ftpClientEx;
    private readonly FileSystemItemViewModel _local;
    private readonly CancellationTokenSource _cts = new();
    private readonly string _newFileRemotePath;

    public UploadFileDialogViewModel()
        : base(WellKnownUri.UndefinedUri) { }

    [ImportingConstructor]
    public UploadFileDialogViewModel(
        ILogService log,
        FtpClientEx ftpClientEx,
        FileSystemItemViewModel? remote,
        FileSystemItemViewModel local
    )
        : base($"{WellKnownUri.ShellPageVehicleFileBrowser}.upload-file")
    {
        _log = log;
        _ftpClientEx = ftpClientEx;
        _local = local;

        if (remote == null)
        {
            _newFileRemotePath = Path.Combine("/", local.Name);
        }
        else if (remote.IsDirectory)
        {
            _newFileRemotePath = Path.Combine(remote.Path, local.Name);
        }
        else if (remote.IsFile)
        {
            _newFileRemotePath = Path.Combine(
                remote.Path[..remote.Path.LastIndexOf('\\')],
                local.Name
            );
        }
    }

    [Reactive] public double UploadProgress { get; set; }

    public void ApplyDialog(ContentDialog dialog)
    {
        dialog.Opened += OnDialogOpened;
        dialog.Closed += OnDialogClosed;
    }

    private async void OnDialogClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
    {
        await _cts.CancelAsync();
    }

    private async void OnDialogOpened(ContentDialog sender, EventArgs args)
    {
        var progress = new Progress<double>();

        progress.ProgressChanged += (_, value) =>
        {
            UploadProgress = value;
        };

        try
        {
            var stream = new FileStream(
                _local.Path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read
            );
            await _ftpClientEx.UploadFile(_newFileRemotePath, stream, progress, _cts.Token);
        }
        catch (OperationCanceledException)
        {
            _log.Warning(
                nameof(VehicleFileBrowserViewModel),
                $"File uploading was canceled: {_local.Name}"
            );
        }
        finally
        {
            sender.Hide();
        }
    }
}
