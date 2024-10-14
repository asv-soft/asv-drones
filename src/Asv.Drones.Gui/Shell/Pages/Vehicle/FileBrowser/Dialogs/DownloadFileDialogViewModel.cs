using System;
using System.Buffers;
using System.Composition;
using System.IO;
using Asv.Drones.Gui.Api;
using Asv.Mavlink;
using FluentAvalonia.UI.Controls;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui;

public class DownloadFileDialogViewModel : ViewModelBaseWithValidation
{
    private readonly FtpClientEx _ftpClientEx;
    private readonly string _localRootPath;
    private readonly FileSystemItemViewModel _remote;
    private readonly FileSystemItemViewModel? _local;

    public DownloadFileDialogViewModel() : base(WellKnownUri.UndefinedUri)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [ImportingConstructor]
    public DownloadFileDialogViewModel( 
        FtpClientEx ftpClientEx, 
        string localRootPath, 
        FileSystemItemViewModel remote, 
        FileSystemItemViewModel? local) 
        : base($"{WellKnownUri.ShellPageVehicleFileBrowser}.upload-file")
    {
        _ftpClientEx = ftpClientEx;
        _localRootPath = localRootPath;
        _remote = remote;
        _local = local;
    }
    
    public void ApplyDialog(ContentDialog dialog)
    {
        dialog.Opened += OnDialogOpened;
    }

    private async void OnDialogOpened(ContentDialog sender, EventArgs args)
    {
        var buffer = new ArrayBufferWriter<byte>();
        var progress = new Progress<double>();

        progress.ProgressChanged += (_, value) =>
        {
            DownloadProgress = value;
        };
        
        await _ftpClientEx.DownloadFile(_remote.Path, buffer, progress);

        var path = _localRootPath;
        if (_local != null)
        {
            path = Path.Combine(_local.IsDirectory ? 
                    _local.Path :
                    _local.Path[.._local.Path.LastIndexOf('\\')], 
                _remote.Name);
        }
        
        await File.WriteAllBytesAsync(path, buffer.WrittenSpan.ToArray());
        Dispose();
    }
    
    [Reactive] public double DownloadProgress { get; set; }
}