using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Avalonia.IO;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Asv.Modeling;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NuGet.Packaging;
using ObservableCollections;
using R3;

namespace Asv.Drones;

public sealed class FileBrowserViewModelConfig
{
    public string LocalSearchText { get; set; } = string.Empty;
    public string RemoteSearchText { get; set; } = string.Empty;
    public string? LocalSelectedItemKey { get; set; }
    public string? RemoteSelectedItemKey { get; set; }
    public IDictionary<string, DirectoryItemViewModelConfig> LocalDirectories { get; set; } =
        new Dictionary<string, DirectoryItemViewModelConfig>();
    public IDictionary<string, DirectoryItemViewModelConfig> RemoteDirectories { get; set; } =
        new Dictionary<string, DirectoryItemViewModelConfig>();
}

public class FileBrowserViewModel
    : DevicePageViewModel<IFileBrowserViewModel>,
        IFileBrowserViewModel,
        ISupportRefresh
{
    public const string PageId = "filesBrowser";
    public const MaterialIconKind PageIcon = MaterialIconKind.FolderEye;
    private const int SearchThrottleMs = 500;

    private readonly LocalFilesService _localFilesService;

    private readonly YesOrNoDialogPrefab _yesNoDialog;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ProgressWithLock _transfer;
    private readonly string _localRootPath;
    private readonly string _remoteRootPath;

    private readonly ObservableDictionary<string, IFtpEntry> _rawRemoteEntries;

    private readonly ObservableList<IBrowserItemViewModel> _localItems;
    private readonly ObservableList<IBrowserItemViewModel> _remoteItems;

    private FtpClientService? _ftpService;
    private FileBrowserBackend? _backend;

    private readonly IUndoChangeSink<ValueUndoChange<string>> _localRenameUndoSink;
    private readonly IUndoChangeSink<ValueUndoChange<string>> _remoteRenameUndoSink;
    private FileBrowserViewModelConfig? _config;
    private bool _localSelectionRestored;
    private bool _remoteSelectionRestored;

    private ILogger Logger => _loggerFactory.CreateLogger(GetType());

    public FileBrowserViewModel()
        : this(
            DesignTime.PageContext,
            NullDeviceManager.Instance,
            AppHost.Instance.Services.GetRequiredService<IAppPath>(),
            NullLoggerFactory.Instance,
            NullDialogService.Instance,
            NullExtensionService.Instance
        )
    {
        RemoteItemsTree = new BrowserTree(
            new ObservableList<IBrowserItemViewModel>
            {
                new DirectoryItemViewModel(
                    NavId.GenerateRandomAsString(),
                    "/",
                    "/folder/",
                    "folder",
                    FtpBrowserSourceType.Remote,
                    _loggerFactory
                ),
                new FileItemViewModel(
                    NavId.GenerateRandomAsString(),
                    "/",
                    "/file.txt",
                    "file.txt",
                    111,
                    FtpBrowserSourceType.Remote,
                    _loggerFactory
                ),
                new FileItemViewModel(
                    NavId.GenerateRandomAsString(),
                    "/",
                    "/file2.txt",
                    "file2.txt",
                    2222,
                    FtpBrowserSourceType.Remote,
                    _loggerFactory
                ),
                new FileItemViewModel(
                    NavId.GenerateRandomAsString(),
                    "/",
                    "/file3.txt",
                    "file3.txt",
                    333333,
                    FtpBrowserSourceType.Remote,
                    _loggerFactory
                ),
                new FileItemViewModel(
                    NavId.GenerateRandomAsString(),
                    "/",
                    "/file4.txt",
                    "file4.txt",
                    44444444,
                    FtpBrowserSourceType.Remote,
                    _loggerFactory
                ),
            },
            "/"
        );
        LocalItemsTree = new BrowserTree(
            new ObservableList<IBrowserItemViewModel>
            {
                new DirectoryItemViewModel(
                    NavId.GenerateRandomAsString(),
                    "/",
                    "/folder/",
                    "folder",
                    FtpBrowserSourceType.Remote,
                    _loggerFactory
                ),
                new FileItemViewModel(
                    NavId.GenerateRandomAsString(),
                    "/",
                    "/file.txt",
                    "file.txt",
                    128,
                    FtpBrowserSourceType.Remote,
                    _loggerFactory
                ),
                new FileItemViewModel(
                    NavId.GenerateRandomAsString(),
                    "/",
                    "/file2.txt",
                    "file2.txt",
                    64544,
                    FtpBrowserSourceType.Remote,
                    _loggerFactory
                ),
                new FileItemViewModel(
                    NavId.GenerateRandomAsString(),
                    "/",
                    "/file3.txt",
                    "file3.txt",
                    23512612,
                    FtpBrowserSourceType.Remote,
                    _loggerFactory
                ),
                new FileItemViewModel(
                    NavId.GenerateRandomAsString(),
                    "/",
                    "/file4.txt",
                    "file4.txt",
                    23423,
                    FtpBrowserSourceType.Remote,
                    _loggerFactory
                ),
            },
            "/"
        );
    }

    public FileBrowserViewModel(
        IPageContext context,
        IDeviceManager devices,
        IAppPath appPath,
        ILoggerFactory loggerFactory,
        IDialogService dialogService,
        IExtensionService ext
    )
        : base(PageId, context, devices, loggerFactory, dialogService, ext)
    {
        _localRootPath = appPath.GetAppPathFolder(string.Empty);
        _remoteRootPath = MavlinkFtpHelper.DirectorySeparator.ToString();
        _loggerFactory = loggerFactory;
        _yesNoDialog = dialogService.GetDialogPrefab<YesOrNoDialogPrefab>();
        _localFilesService = new LocalFilesService();
        _transfer = new ProgressWithLock(loggerFactory).DisposeItWith(Disposable);
        IsTransferInProgress = _transfer
            .IsTransferInProgress.ToBindableReactiveProperty()
            .DisposeItWith(Disposable);
        IsProgressVisible = _transfer
            .IsProgressVisible.ToBindableReactiveProperty()
            .DisposeItWith(Disposable);
        Progress = _transfer.Progress.ToBindableReactiveProperty().DisposeItWith(Disposable);

        var watcher = new FileSystemWatcher(_localRootPath)
        {
            NotifyFilter =
                NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.Size,
            IncludeSubdirectories = true,
            EnableRaisingEvents = true,
        }.DisposeItWith(Disposable);

        Observable
            .Merge(
                Observable.FromEvent<FileSystemEventHandler, Unit>(
                    h => (_, _) => h(Unit.Default),
                    h => watcher.Created += h,
                    h => watcher.Created -= h
                ),
                Observable.FromEvent<FileSystemEventHandler, Unit>(
                    h => (_, _) => h(Unit.Default),
                    h => watcher.Deleted += h,
                    h => watcher.Deleted -= h
                ),
                Observable.FromEvent<FileSystemEventHandler, Unit>(
                    h => (_, _) => h(Unit.Default),
                    h => watcher.Changed += h,
                    h => watcher.Changed -= h
                ),
                Observable.FromEvent<RenamedEventHandler, Unit>(
                    h => (_, _) => h(Unit.Default),
                    h => watcher.Renamed += h,
                    h => watcher.Renamed -= h
                )
            )
            .ThrottleLast(TimeSpan.FromMilliseconds(150))
            .Subscribe(_ => RefreshLocalCommand?.Execute(Unit.Default))
            .DisposeItWith(Disposable);

        _localItems = [];
        _remoteItems = [];
        _localItems.DisposeRemovedItems().DisposeItWith(Disposable);
        _remoteItems.DisposeRemovedItems().DisposeItWith(Disposable);
        _localItems.SetRoutableParent(this).DisposeItWith(Disposable);
        _remoteItems.SetRoutableParent(this).DisposeItWith(Disposable);

        // TODO: The sync may be done by ObservableTree in Asv.Avalonia instead
        _rawRemoteEntries = new ObservableDictionary<string, IFtpEntry>();
        _ = new RemoteEntriesSync(
            _rawRemoteEntries,
            _remoteItems,
            RemoteEntryToBrowserItem,
            _loggerFactory
        ).DisposeItWith(Disposable);

        LocalItemsTree = new BrowserTree(_localItems, _localRootPath).DisposeItWith(Disposable);
        RemoteItemsTree = new BrowserTree(
            _remoteItems,
            MavlinkFtpHelper.DirectorySeparator.ToString()
        ).DisposeItWith(Disposable);

        LocalSelectedItem = new BindableReactiveProperty<BrowserNode?>(null).DisposeItWith(
            Disposable
        );
        RemoteSelectedItem = new BindableReactiveProperty<BrowserNode?>(null).DisposeItWith(
            Disposable
        );

        IsDownloadPopupOpen = new BindableReactiveProperty<bool>(false).DisposeItWith(Disposable);
        IsUiBlocked = new BindableReactiveProperty<bool>(false).DisposeItWith(Disposable);

        CanDownload
            .Where(b => !b)
            .Subscribe(_ => IsDownloadPopupOpen.OnNext(false))
            .DisposeItWith(Disposable);

        LocalSearch = new SearchBoxViewModel(
            nameof(LocalSearch),
            loggerFactory,
            PerformLocalSearch,
            TimeSpan.FromMilliseconds(SearchThrottleMs)
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        RemoteSearch = new SearchBoxViewModel(
            nameof(RemoteSearch),
            loggerFactory,
            PerformRemoteSearch,
            TimeSpan.FromMilliseconds(SearchThrottleMs)
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        InitCommands();

        _localRenameUndoSink = Undo.Register<ValueUndoChange<string>>(
                "local_rename",
                UndoLocalRename,
                RedoLocalRename
            )
            .DisposeItWith(Disposable);
        _remoteRenameUndoSink = Undo.Register<ValueUndoChange<string>>(
                "remote_rename",
                UndoRemoteRename,
                RedoRemoteRename
            )
            .DisposeItWith(Disposable);

        Events.Catch(InternalCatchEvent).DisposeItWith(Disposable);

        Target
            .Where(w => w.HasValue)
            .Select(w => w!.Value)
            .ObserveOnUIThreadDispatcher()
            .Subscribe(w => OnDeviceConnected(w.Device, w.WhenDisconnectedToken))
            .DisposeItWith(Disposable);
    }

    #region Properties

    public BrowserTree LocalItemsTree { get; }
    public BrowserTree RemoteItemsTree { get; }
    public BindableReactiveProperty<BrowserNode?> LocalSelectedItem { get; }
    public BindableReactiveProperty<BrowserNode?> RemoteSelectedItem { get; }
    public SearchBoxViewModel LocalSearch { get; }
    public SearchBoxViewModel RemoteSearch { get; }
    public BindableReactiveProperty<double> Progress { get; }
    public BindableReactiveProperty<bool> IsDownloadPopupOpen { get; }
    public BindableReactiveProperty<bool> IsUiBlocked { get; }
    public BindableReactiveProperty<bool> IsProgressVisible { get; }
    public BindableReactiveProperty<bool> IsTransferInProgress { get; }

    #endregion

    #region Commands

    public ReactiveCommand ShowDownloadPopupCommand { get; private set; }
    public ReactiveCommand<BrowserNode> UploadCommand { get; private set; }
    public ReactiveCommand<BrowserNode> DownloadCommand { get; private set; }
    public ReactiveCommand<BrowserNode> BurstDownloadCommand { get; private set; }
    public ReactiveCommand<BrowserNode?> CreateRemoteFolderCommand { get; private set; }
    public ReactiveCommand<BrowserNode?> CreateLocalFolderCommand { get; private set; }
    public ReactiveCommand RefreshRemoteCommand { get; private set; }
    public ReactiveCommand RefreshLocalCommand { get; private set; }
    public ReactiveCommand<BrowserNode> RemoveLocalItemCommand { get; private set; }
    public ReactiveCommand<BrowserNode> RemoveRemoteItemCommand { get; private set; }
    public ReactiveCommand<BrowserNode> LocalRenameCommand { get; private set; }
    public ReactiveCommand<BrowserNode> RemoteRenameCommand { get; private set; }
    public ReactiveCommand<Unit> CompareSelectedItemsCommand { get; private set; }
    public ReactiveCommand<Unit> FindFileOnLocalCommand { get; private set; }
    public ReactiveCommand<BrowserNode> CalculateLocalCrc32Command { get; private set; }
    public ReactiveCommand<BrowserNode> CalculateRemoteCrc32Command { get; private set; }
    public ReactiveCommand CancelTransferCommand { get; private set; }

    #endregion

    #region Observables

    private Observable<bool> CanUpload => LocalSelectedItem.Select(x => x is not null);

    private Observable<bool> CanDownload => RemoteSelectedItem.Select(x => x is not null);

    private Observable<bool> CanRemoveLocal =>
        LocalSelectedItem.Select(x => x is { Base.EditMode: false });

    private Observable<bool> CanRemoveRemote =>
        RemoteSelectedItem.Select(x => x is { Base.EditMode: false });

    private Observable<bool> CanFindFileOnLocal =>
        RemoteSelectedItem.Select(x =>
            x is { Base: { EditMode: false, FtpEntryType: FtpEntryType.File } }
        );

    private Observable<bool> CanCompareSelectedItems =>
        LocalSelectedItem.CombineLatest(
            RemoteSelectedItem,
            (local, remote) =>
                local is { Base: { EditMode: false, FtpEntryType: FtpEntryType.File } }
                && remote is { Base: { EditMode: false, FtpEntryType: FtpEntryType.File } }
        );

    private Observable<bool> CanCalculateRemoteCrc32 =>
        RemoteSelectedItem.Select(x =>
            x is { Base: { EditMode: false, FtpEntryType: FtpEntryType.File } }
        );

    private Observable<bool> CanCalculateLocalCrc32 =>
        LocalSelectedItem.Select(x =>
            x is { Base: { EditMode: false, FtpEntryType: FtpEntryType.File } }
        );

    private Observable<bool> CanRenameLocal =>
        LocalSelectedItem.Select(x => x is { Base.EditMode: false });

    private Observable<bool> CanRenameRemote =>
        RemoteSelectedItem.Select(x => x is { Base.EditMode: false });

    #endregion

    #region Commands Impl

    private void InitCommands()
    {
        FindFileOnLocalCommand = CanFindFileOnLocal
            .ToReactiveCommand<Unit>(
                async (_, ct) => FindFileOnLocal(),
                awaitOperation: AwaitOperation.Drop
            )
            .DisposeItWith(Disposable);
        CompareSelectedItemsCommand = CanCompareSelectedItems
            .ToReactiveCommand<Unit>(
                async (_, ct) => await CompareSelectedItemsImpl(ct),
                awaitOperation: AwaitOperation.Drop
            )
            .DisposeItWith(Disposable);
        CalculateLocalCrc32Command = CanCalculateLocalCrc32
            .ToReactiveCommand<BrowserNode>(
                async (node, ct) => await CalculateCrc32Impl(node, ct),
                awaitOperation: AwaitOperation.Drop
            )
            .DisposeItWith(Disposable);
        CalculateRemoteCrc32Command = CanCalculateRemoteCrc32
            .ToReactiveCommand<BrowserNode>(
                async (node, ct) => await CalculateCrc32Impl(node, ct),
                awaitOperation: AwaitOperation.Drop
            )
            .DisposeItWith(Disposable);
        RefreshRemoteCommand = new ReactiveCommand(
            async (_, ct) => await RefreshRemoteImpl(ct)
        ).DisposeItWith(Disposable);
        RefreshLocalCommand = new ReactiveCommand(RefreshLocalImpl).DisposeItWith(Disposable);

        LocalRenameCommand = CanRenameLocal
            .ToReactiveCommand<BrowserNode>(SetEditModeImpl, awaitOperation: AwaitOperation.Drop)
            .DisposeItWith(Disposable);
        RemoteRenameCommand = CanRenameRemote
            .ToReactiveCommand<BrowserNode>(
                async (node, ct) => await SetEditModeImpl(node, ct),
                awaitOperation: AwaitOperation.Drop
            )
            .DisposeItWith(Disposable);

        // TODO: Upload incorrectly processes cancellation (prob. something wrong at Asv.Mavlink)
        UploadCommand = CanUpload
            .ToReactiveCommand<BrowserNode>(async (node, ct) => await UploadImpl(node, ct))
            .DisposeItWith(Disposable);
        DownloadCommand = CanDownload
            .ToReactiveCommand<BrowserNode>(
                async (node, ct) =>
                {
                    IsDownloadPopupOpen.OnNext(false);
                    await DownloadImpl(node, ct);
                }
            )
            .DisposeItWith(Disposable);
        BurstDownloadCommand = CanDownload
            .ToReactiveCommand<BrowserNode>(
                async (node, ct) =>
                {
                    IsDownloadPopupOpen.OnNext(false);
                    await BurstDownloadImpl(node, ct);
                }
            )
            .DisposeItWith(Disposable);

        CancelTransferCommand = new ReactiveCommand(_ => _transfer.TryCancel()).DisposeItWith(
            Disposable
        );

        CreateRemoteFolderCommand = new ReactiveCommand<BrowserNode?>(
            async (node, ct) =>
            {
                if (node is null)
                {
                    var item = _remoteItems.FirstOrDefault(x => x.Path.Equals(_remoteRootPath));
                    if (item != null)
                    {
                        await CreateFolderImpl(item, ct);
                    }
                }
                else
                {
                    await CreateFolderImpl(node.Base, ct);
                }
            },
            awaitOperation: AwaitOperation.Drop
        ).DisposeItWith(Disposable);
        CreateLocalFolderCommand = new ReactiveCommand<BrowserNode?>(
            async (node, ct) =>
            {
                if (node is null)
                {
                    var item = _localItems.FirstOrDefault(x => x.Path.Equals(_localRootPath));
                    if (item != null)
                    {
                        await CreateFolderImpl(item, ct);
                    }
                }
                else
                {
                    await CreateFolderImpl(node.Base, ct);
                }
            },
            awaitOperation: AwaitOperation.Drop
        ).DisposeItWith(Disposable);
        RemoveLocalItemCommand = CanRemoveLocal
            .ToReactiveCommand<BrowserNode>(RemoveItemImpl, awaitOperation: AwaitOperation.Drop)
            .DisposeItWith(Disposable);
        RemoveRemoteItemCommand = CanRemoveRemote
            .ToReactiveCommand<BrowserNode>(
                async (node, ct) => await RemoveItemImpl(node, ct),
                awaitOperation: AwaitOperation.Drop
            )
            .DisposeItWith(Disposable);

        RefreshRemoteCommand.IgnoreOnErrorResume(e =>
        {
            if (e is not FtpNackEndOfFileException)
            {
                throw e;
            }
        });

        ShowDownloadPopupCommand = CanDownload
            .ToReactiveCommand(_ => IsDownloadPopupOpen.OnNext(!IsDownloadPopupOpen.Value))
            .DisposeItWith(Disposable);

        RefreshRemoteCommand.SubscribeOnUIThreadDispatcher();
        RefreshLocalCommand.SubscribeOnUIThreadDispatcher();
    }

    private async Task UploadImpl(BrowserNode item, CancellationToken ct)
    {
        string remoteDirectory;

        var remoteNode = RemoteSelectedItem.Value;
        var localName = item.Base.Name;
        const char sep = MavlinkFtpHelper.DirectorySeparator;

        if (remoteNode != null)
        {
            var remotePath = remoteNode.Base.Path;

            if (remoteNode.Base.FtpEntryType is FtpEntryType.Directory)
            {
                remoteDirectory = remotePath + localName;
            }
            else
            {
                var parentDir = remotePath[..remotePath.LastIndexOf(sep)];
                remoteDirectory = parentDir + sep + localName;
            }
        }
        else
        {
            remoteDirectory = sep + localName;
        }

        var payload = new YesOrNoDialogPayload
        {
            Title = RS.FileBrowserViewModel_UploadingDialog_Title,
            Message = string.Format(
                RS.FileBrowserViewModel_UploadingDialog_Message,
                item.Base.Name
            ),
        };

        var res = await _yesNoDialog.ShowDialogAsync(payload);

        if (res)
        {
            await UploadItem(item.Base.Path, remoteDirectory, item.Base.FtpEntryType, ct);
        }
    }

    private async ValueTask DownloadImpl(BrowserNode item, CancellationToken ct)
    {
        if (_ftpService is null)
        {
            return;
        }

        var localDirectory = _localRootPath;

        if (RemoteSelectedItem.Value is null)
        {
            return;
        }

        if (LocalSelectedItem.Value != null)
        {
            if (LocalSelectedItem.Value.Base.HasChildren)
            {
                localDirectory = LocalSelectedItem.Value.Base.Path;
            }
            else
            {
                localDirectory = LocalSelectedItem.Value.Base.Path[
                    ..LocalSelectedItem.Value.Base.Path.LastIndexOf(Path.DirectorySeparatorChar)
                ];
            }
        }

        var payload = new YesOrNoDialogPayload
        {
            Title = RS.FileBrowserViewModel_DownloadDialog_Title,
            Message = string.Format(RS.FileBrowserViewModel_DownloadDialog_Message, item.Base.Name),
        };

        var res = await _yesNoDialog.ShowDialogAsync(payload);

        if (res)
        {
            await DownloadItem(
                item.Base.Path,
                localDirectory,
                MavlinkFtpHelper.MaxDataSize,
                item.Base.FtpEntryType,
                ct
            );
        }
    }

    private async ValueTask BurstDownloadImpl(BrowserNode item, CancellationToken ct)
    {
        var localDirectory = _localRootPath;

        if (LocalSelectedItem.Value != null)
        {
            if (LocalSelectedItem.Value.Base.HasChildren)
            {
                localDirectory = LocalSelectedItem.Value.Base.Path;
            }
            else
            {
                localDirectory = LocalSelectedItem.Value.Base.Path[
                    ..LocalSelectedItem.Value.Base.Path.LastIndexOf(Path.DirectorySeparatorChar)
                ];
            }
        }

        using var viewModel = new BurstDownloadDialogViewModel();
        var dialog = new ContentDialog(viewModel)
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
            var partSize = viewModel.PacketSize.Value ?? MavlinkFtpHelper.MaxDataSize;
            await BurstDownloadItem(
                item.Base.Path,
                localDirectory,
                partSize,
                item.Base.FtpEntryType,
                ct
            );
        }
    }

    private async ValueTask RemoveItemImpl(BrowserNode item, CancellationToken ct)
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

        await item.Base.RemoveAsync(ct);
    }

    private static async ValueTask CreateFolderImpl(
        IBrowserItemViewModel item,
        CancellationToken ct
    )
    {
        await item.CreateDirectoryAsync(ct);
    }

    private async Task RefreshRemoteImpl(CancellationToken ct)
    {
        if (_ftpService is null)
        {
            return;
        }

        var items = await _ftpService.Refresh(ct);

        var selected = RemoteSelectedItem.Value?.Key;
        var toRemove = _rawRemoteEntries.Where(oldItem =>
            items.All(newItem =>
            {
                if (oldItem.Key != newItem.Key)
                {
                    return true;
                }

                if (oldItem.Value is not FtpFile of || newItem.Value is not FtpFile nf)
                {
                    return false;
                }

                return of.Size != nf.Size;
            })
        );
        foreach (var item in toRemove)
        {
            _rawRemoteEntries.Remove(item);
        }

        var toAdd = items.Where(newItem =>
            _rawRemoteEntries.All(oldItem =>
            {
                if (newItem.Key != oldItem.Key)
                {
                    return true;
                }

                if (newItem.Value is not FtpFile nf || oldItem.Value is not FtpFile of)
                {
                    return false;
                }

                return of.Size != nf.Size;
            })
        );
        _rawRemoteEntries.AddRange(toAdd);

        RestoreRemoteSelectionOnce();

        if (selected is not null && RemoteSelectedItem.Value?.Key != selected)
        {
            if (RemoteItemsTree.FindNode(item => item.Key == selected) is BrowserNode node)
            {
                RemoteSelectedItem.Value = node;
            }
        }
    }

    private ValueTask RefreshLocalImpl(Unit arg, CancellationToken ct)
    {
        if (_backend is null)
        {
            return ValueTask.CompletedTask;
        }

        var newItems = _localFilesService.LoadBrowserItems(
            _localRootPath,
            _localRootPath,
            _backend,
            _loggerFactory
        );

        var selected = LocalSelectedItem.Value?.Key;
        var toRemove = _localItems
            .Where(ls => newItems.All(n => n.Path != ls.Path || n.Size != ls.Size))
            .ToArray();

        foreach (var item in toRemove)
        {
            _localItems.Remove(item);
        }

        var existing = new HashSet<(string path, FileSize? size)>(
            _localItems.Select(i => (i.Path, i.Size))
        );

        var toAdd = newItems.Where(n => !existing.Contains((n.Path, n.Size))).ToArray();

        var toDispose = newItems.Where(n => existing.Contains((n.Path, n.Size)));

        _localItems.AddRange(toAdd);
        toDispose.ForEach(i => i.Dispose());

        if (_config is not null)
        {
            ApplyExpansion(toAdd, _config.LocalDirectories);
        }

        RestoreLocalSelectionOnce();

        if (selected is not null && LocalSelectedItem.Value?.Key != selected)
        {
            if (LocalItemsTree.FindNode(item => item.Key == selected) is BrowserNode node)
            {
                LocalSelectedItem.Value = node;
            }
        }

        return ValueTask.CompletedTask;
    }

    private ValueTask SetEditModeImpl(BrowserNode? node, CancellationToken ct)
    {
        if (node?.Base is not BrowserItemViewModel item)
        {
            return ValueTask.CompletedTask;
        }

        item.EditedName.Value = item.Name;
        item.EditMode = true;

        LocalSelectedItem.OnNext(node);

        return ValueTask.CompletedTask;
    }

    private static async ValueTask CalculateCrc32Impl(BrowserNode item, CancellationToken ct)
    {
        if (item.Base is not FileItemViewModel fileItem)
        {
            return;
        }

        await fileItem.CalculateCrc32Async(ct);
    }

    private async ValueTask CompareSelectedItemsImpl(CancellationToken ct)
    {
        if (_ftpService is null)
        {
            return;
        }

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
            await localFileItem.CalculateCrc32Async(ct);
        }
        var localCrc32 = localFileItem.Crc32 ?? 0;

        if (remoteFileItem.Crc32 == null)
        {
            await remoteFileItem.CalculateCrc32Async(ct);
        }
        var remoteCrc32 = remoteFileItem.Crc32 ?? 0;

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

    public void FindFileOnLocal()
    {
        if (RemoteSelectedItem.Value?.Base is not FileItemViewModel remoteFile)
        {
            return;
        }

        var foundNode = LocalItemsTree.FindNode(n =>
            n.Base is FileItemViewModel lf
            && string.Equals(lf.Name, remoteFile.Name, StringComparison.OrdinalIgnoreCase)
        );

        if (foundNode is not BrowserNode node)
        {
            Logger.LogWarning("Local file \"{Header}\" not found", remoteFile.Name);
            return;
        }

        ExpandParents(node);
        LocalSelectedItem.OnNext(node);
    }

    #endregion

    #region Transfer

    public async ValueTask UploadItem(
        string source,
        string destination,
        FtpEntryType type,
        CancellationToken ct
    )
    {
        if (_ftpService is null)
        {
            return;
        }

        using var transfer = _transfer.BeginScope(ct);

        try
        {
            await _ftpService.UploadAsync(
                source,
                destination,
                type,
                transfer.Token,
                transfer.Reporter
            );
        }
        catch (OperationCanceledException)
        {
            Logger.LogWarning("Upload {Path} cancelled by user", source);
        }
    }

    public async ValueTask DownloadItem(
        string source,
        string destination,
        byte partSize,
        FtpEntryType type,
        CancellationToken ct
    )
    {
        if (_ftpService is null)
        {
            return;
        }

        using var transfer = _transfer.BeginScope(ct);
        try
        {
            await _ftpService.DownloadAsync(
                source,
                destination,
                type,
                transfer.Token,
                partSize,
                transfer.Reporter
            );
        }
        catch (OperationCanceledException)
        {
            Logger.LogWarning("Download {Path} cancelled by user", source);
        }
    }

    public async ValueTask BurstDownloadItem(
        string source,
        string destination,
        byte partSize,
        FtpEntryType type,
        CancellationToken ct
    )
    {
        if (_ftpService is null)
        {
            return;
        }
        using var transfer = _transfer.BeginScope(ct);
        try
        {
            await _ftpService.BurstDownloadAsync(
                source,
                destination,
                type,
                transfer.Token,
                partSize,
                transfer.Reporter
            );
        }
        catch (OperationCanceledException)
        {
            Logger.LogWarning("Burst-Download {Path} cancelled by user", source);
        }
    }

    #endregion

    public void Refresh()
    {
        RefreshLocalCommand.Execute(Unit.Default);
        RefreshRemoteCommand.Execute(Unit.Default);
    }

    #region Helpers

    private Task PerformLocalSearch(
        string? query,
        IProgress<double> progress,
        CancellationToken cancel
    )
    {
        PerformSearch(LocalItemsTree, LocalSelectedItem, query);
        return Task.CompletedTask;
    }

    private Task PerformRemoteSearch(
        string? query,
        IProgress<double> progress,
        CancellationToken cancel
    )
    {
        PerformSearch(RemoteItemsTree, RemoteSelectedItem, query);
        return Task.CompletedTask;
    }

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
            && f.Name.Contains(pattern, StringComparison.OrdinalIgnoreCase)
        );

        if (found is not BrowserNode node)
        {
            return;
        }

        ExpandParents(node);
        target.OnNext(node);
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

    private IBrowserItemViewModel RemoteEntryToBrowserItem(string key, IFtpEntry entry)
    {
        if (_backend == null)
        {
            throw new InvalidOperationException("Backend is not initialized");
        }

        const char sep = MavlinkFtpHelper.DirectorySeparator;

        IBrowserItemViewModel vm;

        switch (entry.Type)
        {
            case FtpEntryType.Directory:
            {
                var dirPath = FtpBrowserPath.Normalize(key, true, sep);
                DirectoryItemViewModelConfig? config = null;
                _config?.RemoteDirectories.TryGetValue(dirPath, out config);
                vm = new DirectoryItemViewModel(
                    PathHelper.EncodePathToId(dirPath),
                    FtpBrowserPath.ParentDirOf(dirPath, sep),
                    dirPath,
                    entry.Name,
                    FtpBrowserSourceType.Remote,
                    _loggerFactory,
                    config
                );
                break;
            }
            case FtpEntryType.File:
            {
                var filePath = FtpBrowserPath.Normalize(key, false, sep);
                vm = new FileItemViewModel(
                    PathHelper.EncodePathToId(filePath),
                    FtpBrowserPath.ParentDirOf(filePath, sep),
                    filePath,
                    entry.Name,
                    ((FtpFile)entry).Size,
                    FtpBrowserSourceType.Remote,
                    _loggerFactory
                );
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(entry));
        }

        vm.AttachBackend(_backend);
        return vm;
    }

    #endregion

    #region Routable

    public override IEnumerable<IViewModel> GetChildren()
    {
        yield return LocalSearch;
        yield return RemoteSearch;

        foreach (var item in _localItems)
        {
            yield return item;
        }

        foreach (var item in _remoteItems)
        {
            yield return item;
        }
    }

    public ValueTask Refresh(CancellationToken cancel)
    {
        Refresh();
        return ValueTask.CompletedTask;
    }

    protected override void AfterLoadExtensions()
    {
        var trigger = Observable
            .Merge(
                LocalSelectedItem.Select(_ => Unit.Default),
                RemoteSelectedItem.Select(_ => Unit.Default),
                LocalSearch.Text.ViewValue.Select(_ => Unit.Default),
                RemoteSearch.Text.ViewValue.Select(_ => Unit.Default)
            )
            .ThrottleLast(TimeSpan.FromMilliseconds(SearchThrottleMs));

        LayoutControllerMixin
            .Register(Layout, PageId, LoadLayout, SaveLayout, trigger)
            .DisposeItWith(Disposable);
        Layout.LoadWhenRootAttached(RootTracking).AddTo(ref DisposableBag);
    }

    #endregion

    #region Layout

    private void LoadLayout(FileBrowserViewModelConfig config)
    {
        _config = config;

        LocalSearch.Text.ModelValue.Value = config.LocalSearchText;
        RemoteSearch.Text.ModelValue.Value = config.RemoteSearchText;

        ApplyExpansion(_localItems, config.LocalDirectories);
        ApplyExpansion(_remoteItems, config.RemoteDirectories);

        RestoreLocalSelectionOnce();
        RestoreRemoteSelectionOnce();
    }

    private FileBrowserViewModelConfig SaveLayout()
    {
        return new FileBrowserViewModelConfig
        {
            LocalSearchText = LocalSearch.Text.ViewValue.Value ?? string.Empty,
            RemoteSearchText = RemoteSearch.Text.ViewValue.Value ?? string.Empty,
            LocalSelectedItemKey = LocalSelectedItem.Value?.Key,
            RemoteSelectedItemKey = RemoteSelectedItem.Value?.Key,
            LocalDirectories = CollectExpandedDirectories(_localItems),
            RemoteDirectories = CollectExpandedDirectories(_remoteItems),
        };
    }

    private static IDictionary<string, DirectoryItemViewModelConfig> CollectExpandedDirectories(
        IEnumerable<IBrowserItemViewModel> items
    )
    {
        var result = new Dictionary<string, DirectoryItemViewModelConfig>();
        foreach (
            var item in items.Where(i =>
                i is { FtpEntryType: FtpEntryType.Directory, IsExpanded: true }
            )
        )
        {
            result[item.Path] = new DirectoryItemViewModelConfig { IsExpanded = true };
        }

        return result;
    }

    private static void ApplyExpansion(
        IEnumerable<IBrowserItemViewModel> items,
        IDictionary<string, DirectoryItemViewModelConfig> config
    )
    {
        foreach (var item in items)
        {
            if (
                item.FtpEntryType == FtpEntryType.Directory
                && config.TryGetValue(item.Path, out var dirConfig)
            )
            {
                item.IsExpanded = dirConfig.IsExpanded;
            }
        }
    }

    private void RestoreLocalSelectionOnce()
    {
        if (_localSelectionRestored || _config is null || _localItems.Count == 0)
        {
            return;
        }

        _localSelectionRestored = true;
        if (_config.LocalSelectedItemKey is null)
        {
            return;
        }

        LocalSelectedItem.Value =
            LocalItemsTree.FindNode(n => n.Key == _config.LocalSelectedItemKey) as BrowserNode;
    }

    private void RestoreRemoteSelectionOnce()
    {
        if (_remoteSelectionRestored || _config is null || _remoteItems.Count == 0)
        {
            return;
        }

        _remoteSelectionRestored = true;
        if (_config.RemoteSelectedItemKey is null)
        {
            return;
        }

        RemoteSelectedItem.Value =
            RemoteItemsTree.FindNode(n => n.Key == _config.RemoteSelectedItemKey) as BrowserNode;
    }

    #endregion

    #region Undo

    private ValueTask InternalCatchEvent(
        IViewModel src,
        AsyncRoutedEvent<IViewModel> e,
        CancellationToken cancel
    )
    {
        if (e is BrowserItemRenamedEvent renamed)
        {
            var sink =
                renamed.SourceType == FtpBrowserSourceType.Local
                    ? _localRenameUndoSink
                    : _remoteRenameUndoSink;
            sink.PublishUpdate(renamed.OldPath, renamed.NewPath);
            e.IsHandled = true;
        }

        return ValueTask.CompletedTask;
    }

    private ValueTask UndoLocalRename(ValueUndoChange<string> change, CancellationToken cancel)
    {
        RenameLocal(change.NewValue, change.OldValue);
        return ValueTask.CompletedTask;
    }

    private ValueTask RedoLocalRename(ValueUndoChange<string> change, CancellationToken cancel)
    {
        RenameLocal(change.OldValue, change.NewValue);
        return ValueTask.CompletedTask;
    }

    private void RenameLocal(string from, string to)
    {
        if (Directory.Exists(from))
        {
            _localFilesService.RenameDirectory(from, to, Logger);
        }
        else
        {
            _localFilesService.RenameFile(from, to, Logger);
        }
    }

    private async ValueTask UndoRemoteRename(
        ValueUndoChange<string> change,
        CancellationToken cancel
    )
    {
        await RenameRemoteAsync(change.NewValue, change.OldValue, cancel);
    }

    private async ValueTask RedoRemoteRename(
        ValueUndoChange<string> change,
        CancellationToken cancel
    )
    {
        await RenameRemoteAsync(change.OldValue, change.NewValue, cancel);
    }

    private async ValueTask RenameRemoteAsync(string from, string to, CancellationToken cancel)
    {
        if (_ftpService is null)
        {
            Logger.LogWarning(
                "Cannot undo/redo remote rename \"{From}\" -> \"{To}\": device is disconnected",
                from,
                to
            );
            return;
        }

        await _ftpService.RenameAsync(from, to, cancel);
    }

    #endregion

    #region Dispose


    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _localItems.RemoveAll();
            _remoteItems.RemoveAll();
        }

        base.Dispose(disposing);
    }

    #endregion

    private void OnDeviceConnected(IClientDevice device, CancellationToken onDisconnectedToken)
    {
        Header = $"{RS.FileBrowserViewModel_Title}[{device.Id}]";
        Icon = DeviceIconMixin.GetIcon(device.Id) ?? PageIcon;

        var client = device.GetMicroservice<IFtpClient>();
        ArgumentNullException.ThrowIfNull(client);
        var clientEx = device.GetMicroservice<IFtpClientEx>() ?? new FtpClientEx(client);

        _ftpService = new FtpClientService(clientEx, _loggerFactory).DisposeItWith(Disposable);
        _ftpService.RegisterTo(onDisconnectedToken);

        _ftpService
            .RemoteChanged.ThrottleLast(TimeSpan.FromMilliseconds(200))
            .SubscribeAwait(async (_, _) => await RefreshRemoteImpl(onDisconnectedToken))
            .RegisterTo(onDisconnectedToken);
        _ftpService
            .RemoteChanging.Subscribe(isBusy => IsUiBlocked.OnNext(isBusy))
            .RegisterTo(onDisconnectedToken);

        _backend = new FileBrowserBackend(_localFilesService, _ftpService);

        onDisconnectedToken.Register(() =>
        {
            try
            {
                _transfer.TryCancel();
            }
            catch
            {
                // ignored
            }

            IsUiBlocked.OnNext(false);
            IsProgressVisible.OnNext(false);
            IsTransferInProgress.OnNext(false);
            Progress.OnNext(0);

            _rawRemoteEntries.Clear();
            _remoteItems.Clear();
            RemoteSelectedItem.OnNext(null);
            _ftpService = null;
        });

        Refresh(onDisconnectedToken);
    }
}
