using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Avalonia.IO;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ObservableCollections;
using R3;

namespace Asv.Drones;

[ExportPage(PageId)]
public class FileBrowserViewModel
    : DevicePageViewModel<IFileBrowserViewModel>,
        IFileBrowserViewModel
{
    public const string PageId = "files.browser";
    public const MaterialIconKind PageIcon = MaterialIconKind.FolderEye;

    private readonly YesOrNoDialogPrefab _yesNoDialog;
    private readonly IDialogService _dialogService;
    private readonly ILoggerFactory _loggerFactory;
    private readonly FileSystemWatcher _watcher;
    private readonly INavigationService _navigation;
    private readonly string _localRootPath;
    private readonly FileSystemEventHandler? _createdHandler;
    private readonly FileSystemEventHandler? _deletedHandler;
    private readonly RenamedEventHandler? _renamedHandler;
    private readonly FileSystemEventHandler? _changedHandler;

    private readonly ObservableList<IBrowserItemViewModel> _localItems;
    private readonly ObservableList<IBrowserItemViewModel> _remoteItems;

    public FileBrowserViewModel()
        : this(
            DesignTime.CommandService,
            NullDeviceManager.Instance,
            NullDialogService.Instance,
            NullAppPath.Instance,
            NullLoggerFactory.Instance,
            DesignTime.Navigation
        )
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [ImportingConstructor]
    public FileBrowserViewModel(
        ICommandService cmd,
        IDeviceManager devices,
        IDialogService dialogService,
        IAppPath appPath,
        ILoggerFactory loggerFactory,
        INavigationService navigation
    )
        : base(PageId, devices, cmd, loggerFactory)
    {
        _localRootPath = appPath.UserDataFolder;
        _dialogService = dialogService;
        _loggerFactory = loggerFactory;
        _navigation = navigation;
        _yesNoDialog = dialogService.GetDialogPrefab<YesOrNoDialogPrefab>();

        _watcher = new FileSystemWatcher(_localRootPath)
        {
            NotifyFilter =
                NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.Size,
            IncludeSubdirectories = true,
            EnableRaisingEvents = true,
        };
        _createdHandler = (_, _) => RefreshLocalCommand?.Execute(Unit.Default);
        _deletedHandler = (_, _) => RefreshLocalCommand?.Execute(Unit.Default);
        _renamedHandler = (_, _) => RefreshLocalCommand?.Execute(Unit.Default);
        _changedHandler = (_, _) => RefreshLocalCommand?.Execute(Unit.Default);

        _watcher.Created += _createdHandler;
        _watcher.Deleted += _deletedHandler;
        _watcher.Renamed += _renamedHandler;
        _watcher.Changed += _changedHandler;

        _localItems = [];
        _remoteItems = [];
        _localItems.DisposeRemovedItems(); // TODO: check if all the nodes have proper IRoutableParents. The root node must have this viewmodel as a parent.
        _remoteItems.DisposeRemovedItems();

        LocalItemsTree = new BrowserTree(_localItems, _localRootPath);
        RemoteItemsTree = new BrowserTree(
            _remoteItems,
            MavlinkFtpHelper.DirectorySeparator.ToString()
        );

        var localSearchText = new ReactiveProperty<string?>();
        var remoteSearchText = new ReactiveProperty<string?>();

        LocalSearchText = new HistoricalStringProperty(
            $"{PageId}{nameof(LocalSearchText)}",
            localSearchText,
            loggerFactory
            
        )
        {
            Parent = this,
        };
        RemoteSearchText = new HistoricalStringProperty(
            $"{PageId}{nameof(RemoteSearchText)}",
            remoteSearchText,
            loggerFactory
        )
        {
            Parent = this,
        };

        LocalSelectedItem = new BindableReactiveProperty<BrowserNode?>(null);
        RemoteSelectedItem = new BindableReactiveProperty<BrowserNode?>(null);
        Progress = new BindableReactiveProperty<double>(0);
        IsDownloadPopupOpen = new BindableReactiveProperty<bool>(false);
    }

    private IFtpClientEx ClientEx { get; set; }
    private bool IsClientBusy { get; set; }
    public BrowserTree LocalItemsTree { get; }
    public BrowserTree RemoteItemsTree { get; }
    public BindableReactiveProperty<BrowserNode?> LocalSelectedItem { get; }
    public BindableReactiveProperty<BrowserNode?> RemoteSelectedItem { get; }
    public HistoricalStringProperty LocalSearchText { get; }
    public HistoricalStringProperty RemoteSearchText { get; }
    public BindableReactiveProperty<double> Progress { get; }
    public BindableReactiveProperty<bool> IsDownloadPopupOpen { get; }

    #region Commands

    public ReactiveCommand ShowDownloadPopupCommand { get; private set; }
    public ReactiveCommand<BrowserNode> UploadCommand { get; private set; }
    public ReactiveCommand<BrowserNode> DownloadCommand { get; private set; }
    public ReactiveCommand<BrowserNode> BurstDownloadCommand { get; private set; }
    public ReactiveCommand CreateRemoteFolderCommand { get; private set; }
    public ReactiveCommand CreateLocalFolderCommand { get; private set; }
    public ReactiveCommand RefreshRemoteCommand { get; private set; }
    public ReactiveCommand RefreshLocalCommand { get; private set; }
    public ReactiveCommand<BrowserNode> RemoveLocalItemCommand { get; private set; }
    public ReactiveCommand<BrowserNode> RemoveRemoteItemCommand { get; private set; }
    public ReactiveCommand<BrowserNode> LocalRenameCommand { get; private set; }
    public ReactiveCommand<BrowserNode> RemoteRenameCommand { get; private set; }
    public ReactiveCommand ClearLocalSearchBoxCommand { get; private set; }
    public ReactiveCommand ClearRemoteSearchBoxCommand { get; private set; }
    public ReactiveCommand<Unit> CompareSelectedItemsCommand { get; private set; }
    public ReactiveCommand FindFileOnLocalCommand { get; private set; }
    public ReactiveCommand<BrowserNode> CalculateLocalCrc32Command { get; private set; }
    public ReactiveCommand<BrowserNode> CalculateRemoteCrc32Command { get; private set; }

    private Observable<bool> CanUpload =>
        LocalSelectedItem.Select(x =>
            x?.Base is { FtpEntryType: FtpEntryType.File } && !IsClientBusy
        );

    private Observable<bool> CanDownload =>
        RemoteSelectedItem.Select(x =>
            x?.Base is { FtpEntryType: FtpEntryType.File } && !IsClientBusy
        );

    private Observable<bool> CanRemoveLocal =>
        LocalSelectedItem.Select(x => x?.Base is { IsInEditMode: false });

    private Observable<bool> CanRemoveRemote =>
        RemoteSelectedItem.Select(x => x?.Base is { IsInEditMode: false });

    private Observable<bool> CanFindFileOnLocal =>
        RemoteSelectedItem.Select(x =>
            x?.Base is { IsInEditMode: false, FtpEntryType: FtpEntryType.File }
        );

    private Observable<bool> CanCompareSelectedItems =>
        LocalSelectedItem.CombineLatest(
            RemoteSelectedItem,
            (local, remote) =>
                local?.Base is { IsInEditMode: false, FtpEntryType: FtpEntryType.File }
                && remote?.Base is { IsInEditMode: false, FtpEntryType: FtpEntryType.File }
        );

    private Observable<bool> CanCalculateRemoteCrc32 =>
        RemoteSelectedItem.Select(x =>
            x?.Base is { IsInEditMode: false, FtpEntryType: FtpEntryType.File }
        );

    private Observable<bool> CanCalculateLocalCrc32 =>
        LocalSelectedItem.Select(x =>
            x?.Base is { IsInEditMode: false, FtpEntryType: FtpEntryType.File }
        );

    private Observable<bool> CanRenameLocal =>
        LocalSelectedItem.Select(x => x?.Base is { IsInEditMode: false });

    private Observable<bool> CanRenameRemote =>
        RemoteSelectedItem.Select(x => x?.Base is { IsInEditMode: false });

    #endregion

    #region Commands implementation

    private void InitCommands()
    {
        ClearLocalSearchBoxCommand = new ReactiveCommand(_ =>
            LocalSearchText.ViewValue.OnNext(string.Empty)
        );
        ClearRemoteSearchBoxCommand = new ReactiveCommand(_ =>
            RemoteSearchText.ViewValue.OnNext(string.Empty)
        );
        FindFileOnLocalCommand = CanFindFileOnLocal.ToReactiveCommand(_ => FindFileOnLocalImpl());
        CompareSelectedItemsCommand = CanCompareSelectedItems.ToReactiveCommand<Unit>(
            async (_, ct) => await CompareSelectedItemsImpl(ct),
            awaitOperation: AwaitOperation.Drop
        );
        CalculateLocalCrc32Command = CanCalculateLocalCrc32.ToReactiveCommand<BrowserNode>(
            async (node, ct) => await CalculateLocalCrc32Impl(node, ct),
            awaitOperation: AwaitOperation.Drop
        );
        CalculateRemoteCrc32Command = CanCalculateRemoteCrc32.ToReactiveCommand<BrowserNode>(
            async (node, ct) => await CalculateRemoteCrc32Impl(node, ct),
            awaitOperation: AwaitOperation.Drop
        );
        RefreshRemoteCommand = new ReactiveCommand(async (_, ct) => await RefreshRemoteImpl(ct));
        RefreshLocalCommand = new ReactiveCommand(RefreshLocalImpl);

        LocalRenameCommand = CanRenameLocal.ToReactiveCommand<BrowserNode>(
            LocalRenameImpl,
            awaitOperation: AwaitOperation.Drop
        );
        RemoteRenameCommand = CanRenameRemote.ToReactiveCommand<BrowserNode>(
            async (node, ct) =>
                await RemoteRenameImpl(node, ct).ContinueWith(_ => RefreshRemoteImpl(ct), ct),
            awaitOperation: AwaitOperation.Drop
        );

        UploadCommand = CanUpload.ToReactiveCommand<BrowserNode>(
            async (node, ct) =>
            {
                await UploadImpl(node, ct).ContinueWith(_ => RefreshRemoteImpl(ct), ct);
                IsClientBusy = false;
            }
        );
        DownloadCommand = CanDownload.ToReactiveCommand<BrowserNode>(
            async (node, ct) =>
            {
                await DownloadImpl(node, ct);
                IsClientBusy = false;
            }
        );
        BurstDownloadCommand = CanDownload.ToReactiveCommand<BrowserNode>(
            async (node, ct) =>
            {
                await BurstDownloadImpl(node, ct);
                IsClientBusy = false;
            }
        );
        CreateRemoteFolderCommand = new ReactiveCommand(
            async (_, ct) =>
                await CreateRemoteFolderImpl(ct).ContinueWith(_ => RefreshRemoteImpl(ct), ct),
            awaitOperation: AwaitOperation.Drop
        );
        CreateLocalFolderCommand = new ReactiveCommand(
            CreateLocalFolderImpl,
            awaitOperation: AwaitOperation.Drop
        );
        RemoveLocalItemCommand = CanRemoveLocal.ToReactiveCommand<BrowserNode>(
            RemoveLocalItemImpl,
            awaitOperation: AwaitOperation.Drop
        );
        RemoveRemoteItemCommand = CanRemoveRemote.ToReactiveCommand<BrowserNode>(
            async (node, ct) =>
                await RemoveRemoteItemImpl(node, ct).ContinueWith(_ => RefreshRemoteImpl(ct), ct),
            awaitOperation: AwaitOperation.Drop
        );

        RefreshRemoteCommand.IgnoreOnErrorResume(e =>
        {
            if (e is not FtpNackEndOfFileException)
            {
                throw e;
            }
        });

        ShowDownloadPopupCommand = CanDownload.ToReactiveCommand(_ =>
            IsDownloadPopupOpen.OnNext(!IsDownloadPopupOpen.Value)
        );

        _sub1 = CanDownload.Subscribe(b =>
        {
            if (!b)
            {
                IsDownloadPopupOpen.OnNext(false);
            }
        });

        _sub2 = LocalSearchText // TODO: Remove from this method everything that is not about commands initialization
            .ViewValue.DistinctUntilChanged()
            .ThrottleLast(TimeSpan.FromMilliseconds(500)) // TODO: make constants for common values
            .Subscribe(text => PerformSearch(LocalItemsTree, LocalSelectedItem, text));

        _sub3 = RemoteSearchText
            .ViewValue.DistinctUntilChanged()
            .ThrottleLast(TimeSpan.FromMilliseconds(500)) // TODO: make constants for common values
            .Subscribe(text => PerformSearch(RemoteItemsTree, RemoteSelectedItem, text));

        RefreshRemoteCommand.SubscribeOnUIThreadDispatcher();
        RefreshLocalCommand.SubscribeOnUIThreadDispatcher();

        RefreshRemoteCommand.Execute(Unit.Default);
        RefreshLocalCommand.Execute(Unit.Default);
    }

    private async Task UploadImpl(BrowserNode item, CancellationToken ct)
    {
        var payload = new YesOrNoDialogPayload
        {
            Title = RS.FileBrowserViewModel_UploadingDialog_Title,
            Message = string.Format(
                RS.FileBrowserViewModel_UploadingDialog_Message,
                item.Base.Header
            ),
        };

        var res = await _yesNoDialog.ShowDialogAsync(payload);

        if (res)
        {
            IsClientBusy = true;

            await using var stream = new FileStream(
                item.Base.Path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read
            );
            string path;
            if (RemoteSelectedItem.Value != null)
            {
                path = RemoteSelectedItem.Value.Base.HasChildren
                    ? RemoteSelectedItem.Value.Base.Path
                        + $"{LocalSelectedItem.Value?.Base.Header ?? "unknown"}"
                    : RemoteSelectedItem.Value.Base.Path[
                        ..RemoteSelectedItem.Value.Base.Path.LastIndexOf(
                            MavlinkFtpHelper.DirectorySeparator
                        )
                    ]
                        + $"{MavlinkFtpHelper.DirectorySeparator}"
                        + $"{LocalSelectedItem.Value?.Base.Header ?? "unknown"}";
            }
            else
            {
                path =
                    $"{MavlinkFtpHelper.DirectorySeparator}"
                    + $"{LocalSelectedItem.Value?.Base.Header ?? "unknown"}";
            }

            await ClientEx.UploadFile(
                path,
                stream,
                new Progress<double>(i =>
                {
                    if (!Progress.IsCompletedOrDisposed)
                    {
                        Progress.OnNext(i);
                    }
                }),
                ct
            );
        }
    }

    private async ValueTask DownloadImpl(BrowserNode item, CancellationToken ct)
    {
        var path = _localRootPath;

        if (LocalSelectedItem.Value != null)
        {
            if (LocalSelectedItem.Value.Base.HasChildren)
            {
                path = LocalSelectedItem.Value.Base.Path;
            }
            else
            {
                path = LocalSelectedItem.Value.Base.Path[
                    ..LocalSelectedItem.Value.Base.Path.LastIndexOf(Path.DirectorySeparatorChar)
                ];
            }
        }

        var payload = new YesOrNoDialogPayload
        {
            Title = RS.FileBrowserViewModel_DownloadDialog_Title,
            Message = string.Format(
                RS.FileBrowserViewModel_DownloadDialog_Message,
                item.Base.Header
            ),
        };

        var res = await _yesNoDialog.ShowDialogAsync(payload);

        if (res)
        {
            IsClientBusy = true;

            await using MemoryStream stream = new();

            await ClientEx.DownloadFile(
                item.Base.Path,
                stream,
                new Progress<double>(i =>
                {
                    if (!Progress.IsCompletedOrDisposed)
                    {
                        Progress.OnNext(i);
                    }
                }),
                cancel: ct
            );
            await LocalFilesMixin.WriteFileAsync(
                path,
                RemoteSelectedItem.Value!.Base.Header!, // TODO: ! is unacceptable
                stream.ToArray(),
                ct
            );
        }
    }

    private async ValueTask BurstDownloadImpl(BrowserNode item, CancellationToken ct)
    {
        var path = _localRootPath;

        if (LocalSelectedItem.Value != null)
        {
            if (LocalSelectedItem.Value.Base.HasChildren)
            {
                path = LocalSelectedItem.Value.Base.Path;
            }
            else
            {
                path = LocalSelectedItem.Value.Base.Path[
                    ..LocalSelectedItem.Value.Base.Path.LastIndexOf(Path.DirectorySeparatorChar)
                ];
            }
        }

        using var viewModel = new BurstDownloadDialogViewModel();
        var dialog = new ContentDialog(viewModel, _navigation)
        {
            Title = RS.FileBrowserViewModel_BurstDownloadDialog_Title,
            PrimaryButtonText = RS.FileBrowserViewModel_BurstDownloadDialog_PrimaryButtonText,
            SecondaryButtonText = RS.FileBrowserViewModel_BurstDownloadDialog_SecondaryButtonText,
            IsPrimaryButtonEnabled = viewModel.IsValid.Value,
            IsSecondaryButtonEnabled = true,
        };
        viewModel.ApplyDialog(dialog);
        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            IsClientBusy = true;

            await using MemoryStream stream = new();
            var size = viewModel.PacketSize.Value ?? MavlinkFtpHelper.MaxDataSize;

            await ClientEx.BurstDownloadFile(
                item.Base.Path,
                stream,
                new Progress<double>(i =>
                {
                    if (!Progress.IsCompletedOrDisposed)
                    {
                        Progress.OnNext(i);
                    }
                }),
                size,
                ct
            );
            await LocalFilesMixin.WriteFileAsync(
                path,
                RemoteSelectedItem.Value!.Base.Header!,
                stream.ToArray(),
                ct,
                Logger
            );
        }
    }

    private async ValueTask RemoveLocalItemImpl(BrowserNode item, CancellationToken ct)
    {
        var payload = new YesOrNoDialogPayload
        {
            Title = RS.FileBrowserViewModel_RemoveDialog_Title,
            Message = RS.FileBrowserViewModel_RemoveDialog_Message,
        };

        var res = await _yesNoDialog.ShowDialogAsync(payload);
        if (!res)
        {
            return;
        }

        if (item.Base is { FtpEntryType: FtpEntryType.Directory })
        {
            LocalFilesMixin.RemoveDirectory(item.Base.Path, true, Logger);
        }

        if (item.Base is { FtpEntryType: FtpEntryType.File })
        {
            LocalFilesMixin.RemoveFile(item.Base.Path, Logger);
        }
    }

    private async Task RemoveRemoteItemImpl(BrowserNode item, CancellationToken ct)
    {
        var payload = new YesOrNoDialogPayload
        {
            Title = RS.FileBrowserViewModel_RemoveDialog_Title,
            Message = RS.FileBrowserViewModel_RemoveDialog_Message,
        };

        var res = await _yesNoDialog.ShowDialogAsync(payload);
        if (!res)
        {
            return;
        }

        switch (item.Base)
        {
            case { FtpEntryType: FtpEntryType.Directory }:
                await ClientEx.RemoveDirectoryAsync(item.Base.Path, true, ct, Logger);
                break;
            case { FtpEntryType: FtpEntryType.File }:
                await ClientEx.RemoveFileAsync(item.Base.Path, ct, Logger);
                break;
        }
    }

    private async Task CreateRemoteFolderImpl(CancellationToken ct)
    {
        string path;

        if (RemoteSelectedItem.Value != null)
        {
            if (RemoteSelectedItem.Value!.Base.FtpEntryType == FtpEntryType.Directory)
            {
                path = RemoteSelectedItem.Value.Base.Path;
            }
            else
            {
                path = RemoteSelectedItem.Value.Base.Path[
                    ..RemoteSelectedItem.Value.Base.Path.LastIndexOf(
                        MavlinkFtpHelper.DirectorySeparator
                    )
                ];
            }
        }
        else
        {
            path = $"{MavlinkFtpHelper.DirectorySeparator}";
        }

        await ClientEx.CreateDirectoryAsync(path, ct, Logger);
    }

    private ValueTask CreateLocalFolderImpl(Unit arg, CancellationToken ct)
    {
        string path;

        if (LocalSelectedItem.Value != null) // TODO: Try to shorten if else
        {
            if (LocalSelectedItem.Value.Base.FtpEntryType == FtpEntryType.Directory)
            {
                path = LocalSelectedItem.Value.Base.Path;
            }
            else
            {
                path = LocalSelectedItem.Value.Base.Path[
                    ..LocalSelectedItem.Value.Base.Path.LastIndexOf(Path.DirectorySeparatorChar)
                ];
            }
        }
        else
        {
            path = _localRootPath;
        }

        LocalFilesMixin.CreateDirectory(path, Logger);

        return ValueTask.CompletedTask;
    }

    private async Task RefreshRemoteImpl(CancellationToken ct)
    {
        await ClientEx.Refresh(MavlinkFtpHelper.DirectorySeparator.ToString(), cancel: ct);
        var newItems = ClientEx.CopyEntriesAsBrowserItems(_loggerFactory);

        var toRemove = _remoteItems
            .Where(rs => newItems.All(n => n.Path != rs.Path || n.Size != rs.Size))
            .ToList();
        foreach (var item in toRemove)
        {
            _remoteItems.Remove(item);
        }

        var toAdd = newItems
            .Where(n => _remoteItems.All(rs => rs.Path != n.Path || rs.Size != n.Size))
            .ToList();
        _remoteItems.AddRange(toAdd);
    }

    private ValueTask RefreshLocalImpl(Unit arg, CancellationToken ct)
    {
        var newItems = LocalFilesMixin.LoadBrowserItems(_localRootPath, _localRootPath, loggerFactory:_loggerFactory);

        var toRemove = _localItems
            .Where(ls => newItems.All(n => n.Path != ls.Path || n.Size != ls.Size))
            .ToList();
        foreach (var item in toRemove)
        {
            _localItems.Remove(item);
        }

        var toAdd = newItems
            .Where(n => _localItems.All(ls => ls.Path != n.Path || ls.Size != n.Size))
            .ToList();
        _localItems.AddRange(toAdd);

        return ValueTask.CompletedTask;
    }

    private async ValueTask LocalRenameImpl(BrowserNode? node, CancellationToken ct)
    {
        if (node?.Base is not BrowserItemViewModel item)
        {
            return;
        }

        var oldName = item.Header;
        var oldPath = item.Path;

        using var viewModel = new RenameDialogViewModel();
        var dialog = new ContentDialog(viewModel, _navigation)
        {
            Title = RS.FileBrowserViewModel_RenameDialog_Title,
            PrimaryButtonText = RS.FileBrowserViewModel_RenameDialog_PrimaryButtonText,
            SecondaryButtonText = RS.FileBrowserViewModel_RenameDialog_SecondaryButtonText,
            IsSecondaryButtonEnabled = true,
        };

        if (oldName != null)
        {
            viewModel.NewName.OnNext(oldName);
        }

        viewModel.ApplyDialog(dialog);
        var result = await dialog.ShowAsync();
        var newName = viewModel.NewName.Value;

        if (result != ContentDialogResult.Primary)
        {
            return;
        }

        if (string.Equals(newName, oldName, StringComparison.Ordinal))
        {
            return;
        }

        try
        {
            var newPath =
                item.FtpEntryType == FtpEntryType.Directory
                    ? LocalFilesMixin.RenameDirectory(oldPath, newName, Logger)
                    : LocalFilesMixin.RenameFile(oldPath, newName, Logger);

            var newNode = LocalItemsTree.FindNode(n => n.Base.Path == newPath);
            if (newNode != null)
            {
                LocalSelectedItem.OnNext(newNode as BrowserNode);
            }

            Logger.LogInformation("Local item {oldName} renamed to {newName}", oldName, newName);
        }
        catch (Exception ex)
        {
            Logger.LogError(
                ex,
                "Failed to rename local item {oldName} to {newName}",
                oldName,
                newName
            );
        }
    }

    private async Task RemoteRenameImpl(BrowserNode? node, CancellationToken ct)
    {
        if (node?.Base is not BrowserItemViewModel item)
        {
            return;
        }

        var oldName = item.Header;
        var oldPath = item.Path;

        using var viewModel = new RenameDialogViewModel();
        var dialog = new ContentDialog(viewModel, _navigation)
        {
            Title = RS.FileBrowserViewModel_RenameDialog_Title,
            PrimaryButtonText = RS.FileBrowserViewModel_RenameDialog_PrimaryButtonText,
            SecondaryButtonText = RS.FileBrowserViewModel_RenameDialog_SecondaryButtonText,
            IsSecondaryButtonEnabled = true,
            Content = viewModel,
        };

        if (oldName != null)
        {
            viewModel.NewName.OnNext(oldName);
        }

        viewModel.ApplyDialog(dialog);
        var result = await dialog.ShowAsync();
        var newName = viewModel.NewName.Value;

        if (result != ContentDialogResult.Primary)
        {
            return;
        }

        if (string.Equals(newName, oldName, StringComparison.Ordinal))
        {
            return;
        }

        try
        {
            var newPath = await ClientEx.RenameAsync(oldPath, newName, ct, Logger);

            var newNode = RemoteItemsTree.FindNode(n => n.Base.Path == newPath);
            if (newNode != null)
            {
                RemoteSelectedItem.OnNext(newNode as BrowserNode);
            }

            Logger.LogInformation("Remote item {oldName} renamed to {newName}", oldName, newName);
        }
        catch (Exception ex)
        {
            Logger.LogError(
                ex,
                "Failed to rename remote item {oldName} to {newName}",
                oldName,
                newName
            );
        }
    }

    private async ValueTask CalculateLocalCrc32Impl(BrowserNode item, CancellationToken ct)
    {
        if (item.Base is not FileItemViewModel fileItem)
        {
            return;
        }

        var crc32 = await LocalFilesMixin.CalculateCrc32Async(fileItem.Path, ct, Logger);
        fileItem.Crc32 = crc32;
        fileItem.Crc32Status = Crc32Status.Default;
    }

    private async ValueTask CalculateRemoteCrc32Impl(BrowserNode item, CancellationToken ct)
    {
        if (item.Base is not FileItemViewModel fileItem)
        {
            return;
        }

        var crc32 = await ClientEx.CalculateCrc32Async(fileItem.Path, ct, Logger);
        fileItem.Crc32 = crc32;
        fileItem.Crc32Status = Crc32Status.Default;
    }

    private async ValueTask CompareSelectedItemsImpl(CancellationToken ct)
    {
        uint localCrc32;
        uint remoteCrc32;

        if (LocalSelectedItem.Value?.Base is not FileItemViewModel localFileItem)
        {
            return;
        }

        if (RemoteSelectedItem.Value?.Base is not FileItemViewModel remoteFileItem)
        {
            return;
        }

        if (localFileItem.Crc32 == null)
        {
            localCrc32 = await LocalFilesMixin.CalculateCrc32Async(localFileItem.Path, ct, Logger);
            localFileItem.Crc32 = localCrc32;
        }
        else
        {
            localCrc32 = (uint)localFileItem.Crc32;
        }

        if (remoteFileItem.Crc32 == null)
        {
            remoteCrc32 = await ClientEx.Base.CalcFileCrc32(remoteFileItem.Path, ct);
            remoteFileItem.Crc32 = remoteCrc32;
        }
        else
        {
            remoteCrc32 = (uint)remoteFileItem.Crc32;
        }

        if (localCrc32 == remoteCrc32 && (localCrc32 != 0 || remoteCrc32 != 0))
        {
            localFileItem.Crc32Status = Crc32Status.Correct;
            remoteFileItem.Crc32Status = Crc32Status.Correct;
        }
        else
        {
            localFileItem.Crc32Status = Crc32Status.Incorrect;
            remoteFileItem.Crc32Status = Crc32Status.Incorrect;
        }
    }

    private void FindFileOnLocalImpl()
    {
        if (RemoteSelectedItem.Value?.Base is not FileItemViewModel remoteFile)
        {
            return;
        }

        var foundNode = LocalItemsTree.FindNode(n =>
            n.Base is FileItemViewModel lf
            && string.Equals(lf.Header, remoteFile.Header, StringComparison.OrdinalIgnoreCase)
        );

        if (foundNode == null)
        {
            Logger.LogWarning("Local file \"{Header}\" not found", remoteFile.Header);
            return;
        }

        ExpandParents((BrowserNode)foundNode);
        LocalSelectedItem.OnNext((BrowserNode)foundNode);
    }

    #endregion

    private static void PerformSearch(
        BrowserTree tree,
        BindableReactiveProperty<BrowserNode?> target,
        string? pattern
    )
    {
        if (string.IsNullOrWhiteSpace(pattern))
        {
            return;
        }

        var found = tree.FindNode(n =>
            n.Base is BrowserItemViewModel f
            && f.Header?.Contains(pattern, StringComparison.OrdinalIgnoreCase) == true
        );

        if (found == null)
        {
            return;
        }

        ExpandParents((BrowserNode)found); // TODO: This is dangerous Define uneven cast as I do in ValidationValue with errors
        target.OnNext((BrowserNode)found);
    }

    private static void ExpandParents(BrowserNode node)
    {
        foreach (var ancestor in node.GetAllMenuFromRoot())
        {
            switch (ancestor.Base)
            {
                case BrowserItemViewModel bi:
                {
                    if (bi.IsExpanded)
                    {
                        bi.IsExpanded = false; // reset field to trigger UI
                    }

                    bi.IsExpanded = true;
                    break;
                }
            }
        }
    }

    public override ValueTask<IRoutable> Navigate(NavigationId id)
    {
        return ValueTask.FromResult<IRoutable>(this);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    protected override void AfterLoadExtensions() { }

   
    public override IExportInfo Source => SystemModule.Instance;

    #region Dispose

    private IDisposable _sub1;
    private IDisposable _sub2;
    private IDisposable _sub3;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            ClientEx.Base.ResetSessions();

            _watcher.Created -= _createdHandler;
            _watcher.Deleted -= _deletedHandler;
            _watcher.Renamed -= _renamedHandler;
            _watcher.Changed -= _changedHandler;
            _watcher.Dispose();

            _sub1.Dispose();
            _sub2.Dispose();
            _sub3.Dispose();

            ShowDownloadPopupCommand.Dispose();
            UploadCommand.Dispose();
            DownloadCommand.Dispose();
            BurstDownloadCommand.Dispose();
            CreateRemoteFolderCommand.Dispose();
            CreateLocalFolderCommand.Dispose();
            RefreshRemoteCommand.Dispose();
            RefreshLocalCommand.Dispose();
            RemoveLocalItemCommand.Dispose();
            RemoveRemoteItemCommand.Dispose();
            LocalRenameCommand.Dispose();
            RemoteRenameCommand.Dispose();
            ClearLocalSearchBoxCommand.Dispose();
            ClearRemoteSearchBoxCommand.Dispose();
            FindFileOnLocalCommand.Dispose();
            CompareSelectedItemsCommand.Dispose();
            CalculateLocalCrc32Command.Dispose();
            CalculateRemoteCrc32Command.Dispose();

            Progress.Dispose();
            IsDownloadPopupOpen.Dispose();
            LocalSearchText.Dispose();
            RemoteSearchText.Dispose();
            LocalSelectedItem.Dispose();
            RemoteSelectedItem.Dispose();

            LocalItemsTree.Dispose();
            RemoteItemsTree.Dispose();

            _localItems.RemoveAll();
            _remoteItems.RemoveAll();
        }

        base.Dispose(disposing);
    }

    #endregion

    protected override void AfterDeviceInitialized(IClientDevice device, CancellationToken onDisconnectedToken)
    {
        Title = $"Browser[{device.Id}]";
        var client =
            device.GetMicroservice<IFtpClient>()
            ?? throw new MissingMemberException("FTP Client is null");
        ArgumentNullException.ThrowIfNull(client);
        ClientEx = device.GetMicroservice<IFtpClientEx>() ?? new FtpClientEx(client);

        InitCommands();
    }
}
