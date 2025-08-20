using System;
using System.Collections.Generic;
using System.Composition;
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
using Material.Icons;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NuGet.Packaging;
using ObservableCollections;
using R3;
using IConfiguration = Asv.Cfg.IConfiguration;

namespace Asv.Drones;

public sealed class FileBrowserViewModelConfig : PageConfig { }

[ExportPage(PageId)]
public class FileBrowserViewModel
    : DevicePageViewModel<IFileBrowserViewModel, FileBrowserViewModelConfig>,
        IFileBrowserViewModel
{
    public const string PageId = "files.browser";
    public const MaterialIconKind PageIcon = MaterialIconKind.FolderEye;
    private const int SearchThrottleMs = 500;

    private FtpClientService? _ftpService;
    private readonly YesOrNoDialogPrefab _yesNoDialog;
    private readonly ILoggerFactory _loggerFactory;
    private readonly INavigationService _navigation;
    private readonly FileSystemWatcher _watcher;
    private readonly TransferController _transfer;
    private readonly string _localRootPath;
    private readonly FileSystemEventHandler? _createdHandler;
    private readonly FileSystemEventHandler? _deletedHandler;
    private readonly RenamedEventHandler? _renamedHandler;
    private readonly FileSystemEventHandler? _changedHandler;

    private readonly ObservableDictionary<string, IFtpEntry> _rawRemoteEntries;

    private readonly ObservableList<IBrowserItemViewModel> _localItems;
    private readonly ObservableList<IBrowserItemViewModel> _remoteItems;

    public FileBrowserViewModel()
        : this(
            DesignTime.CommandService,
            NullDeviceManager.Instance,
            NullDialogService.Instance,
            NullAppPath.Instance,
            DesignTime.Configuration,
            NullLoggerFactory.Instance,
            DesignTime.Navigation
        )
    {
        DesignTime.ThrowIfNotDesignMode();
        TimeProvider
            .System.CreateTimer(
                _ => IsDeviceInitialized = true,
                null,
                TimeSpan.FromSeconds(5),
                Timeout.InfiniteTimeSpan
            )
            .DisposeItWith(Disposable);
    }

    [ImportingConstructor]
    public FileBrowserViewModel(
        ICommandService cmd,
        IDeviceManager devices,
        IDialogService dialogService,
        IAppPath appPath,
        IConfiguration cfg,
        ILoggerFactory loggerFactory,
        INavigationService navigation
    )
        : base(PageId, devices, cmd, cfg, loggerFactory)
    {
        _localRootPath = appPath.UserDataFolder;
        _loggerFactory = loggerFactory;
        _navigation = navigation;
        _yesNoDialog = dialogService.GetDialogPrefab<YesOrNoDialogPrefab>();
        _transfer = new TransferController(loggerFactory).DisposeItWith(Disposable);
        IsTransferInProgress = _transfer.IsTransferInProgress;
        IsProgressVisible = _transfer.IsProgressVisible;
        Progress = _transfer.Progress;

        _watcher = new FileSystemWatcher(_localRootPath)
        {
            NotifyFilter =
                NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.Size,
            IncludeSubdirectories = true,
            EnableRaisingEvents = true,
        }.DisposeItWith(Disposable);
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
        _localItems.DisposeRemovedItems().DisposeItWith(Disposable);
        _remoteItems.DisposeRemovedItems().DisposeItWith(Disposable);
        _localItems.SetRoutableParent(this);
        _remoteItems.SetRoutableParent(this);

        // TODO: The sync may be done by ObservableTree in Asv.Avalonia instead
        _rawRemoteEntries = new ObservableDictionary<string, IFtpEntry>();
        var remoteSync = new RemoteEntriesSync(
            _rawRemoteEntries,
            _remoteItems,
            EntryToBrowserItem,
            _loggerFactory
        ).DisposeItWith(Disposable);
        remoteSync.Start();

        LocalItemsTree = new BrowserTree(_localItems, _localRootPath).DisposeItWith(Disposable);
        RemoteItemsTree = new BrowserTree(
            _remoteItems,
            MavlinkFtpHelper.DirectorySeparator.ToString()
        ).DisposeItWith(Disposable);

        var localSearchText = new ReactiveProperty<string?>();
        var remoteSearchText = new ReactiveProperty<string?>();

        LocalSearchText = new HistoricalStringProperty(
            nameof(LocalSearchText),
            localSearchText,
            loggerFactory,
            this
        ).DisposeItWith(Disposable);
        LocalSearchText.ForceValidate();

        RemoteSearchText = new HistoricalStringProperty(
            nameof(RemoteSearchText),
            remoteSearchText,
            loggerFactory,
            this
        ).DisposeItWith(Disposable);
        RemoteSearchText.ForceValidate();

        LocalSelectedItem = new BindableReactiveProperty<BrowserNode?>(null).DisposeItWith(
            Disposable
        );
        RemoteSelectedItem = new BindableReactiveProperty<BrowserNode?>(null).DisposeItWith(
            Disposable
        );

        IsDownloadPopupOpen = new BindableReactiveProperty<bool>(false).DisposeItWith(Disposable);
        IsUiBlocked = new BindableReactiveProperty<bool>(false).DisposeItWith(Disposable);

        CanDownload
            .Subscribe(b =>
            {
                if (!b)
                {
                    IsDownloadPopupOpen.OnNext(false);
                }
            })
            .DisposeItWith(Disposable);

        LocalSearchText
            .ViewValue.DistinctUntilChanged()
            .ThrottleLast(TimeSpan.FromMilliseconds(SearchThrottleMs))
            .Subscribe(text => PerformSearch(LocalItemsTree, LocalSelectedItem, text))
            .DisposeItWith(Disposable);

        RemoteSearchText
            .ViewValue.DistinctUntilChanged()
            .ThrottleLast(TimeSpan.FromMilliseconds(SearchThrottleMs))
            .Subscribe(text => PerformSearch(RemoteItemsTree, RemoteSelectedItem, text))
            .DisposeItWith(Disposable);

        IsDeviceInitialized = false;

        InitCommands();
    }

    public BrowserTree LocalItemsTree { get; }
    public BrowserTree RemoteItemsTree { get; }
    public BindableReactiveProperty<BrowserNode?> LocalSelectedItem { get; }
    public BindableReactiveProperty<BrowserNode?> RemoteSelectedItem { get; }
    public HistoricalStringProperty LocalSearchText { get; }
    public HistoricalStringProperty RemoteSearchText { get; }
    public BindableReactiveProperty<double> Progress { get; }
    public BindableReactiveProperty<bool> IsDownloadPopupOpen { get; }
    public BindableReactiveProperty<bool> IsUiBlocked { get; }
    public BindableReactiveProperty<bool> IsProgressVisible { get; }
    public BindableReactiveProperty<bool> IsTransferInProgress { get; }

    public bool IsDeviceInitialized
    {
        get;
        set => SetField(ref field, value);
    }

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

    public ReactiveCommand CancelTransferCommand { get; private set; }

    #endregion

    #region Commands implementation

    private void InitCommands()
    {
        ClearLocalSearchBoxCommand = new ReactiveCommand(_ =>
            LocalSearchText.ViewValue.OnNext(string.Empty)
        ).DisposeItWith(Disposable);
        ClearRemoteSearchBoxCommand = new ReactiveCommand(_ =>
            RemoteSearchText.ViewValue.OnNext(string.Empty)
        ).DisposeItWith(Disposable);
        FindFileOnLocalCommand = CanFindFileOnLocal
            .ToReactiveCommand(_ => FindFileOnLocalImpl())
            .DisposeItWith(Disposable);
        CompareSelectedItemsCommand = CanCompareSelectedItems
            .ToReactiveCommand<Unit>(
                async (_, ct) => await CompareSelectedItemsImpl(ct),
                awaitOperation: AwaitOperation.Drop
            )
            .DisposeItWith(Disposable);
        CalculateLocalCrc32Command = CanCalculateLocalCrc32
            .ToReactiveCommand<BrowserNode>(
                async (node, ct) => await CalculateLocalCrc32Impl(node, ct),
                awaitOperation: AwaitOperation.Drop
            )
            .DisposeItWith(Disposable);
        CalculateRemoteCrc32Command = CanCalculateRemoteCrc32
            .ToReactiveCommand<BrowserNode>(
                async (node, ct) => await CalculateRemoteCrc32Impl(node, ct),
                awaitOperation: AwaitOperation.Drop
            )
            .DisposeItWith(Disposable);
        RefreshRemoteCommand = new ReactiveCommand(
            async (_, ct) => await RefreshRemoteImpl(ct)
        ).DisposeItWith(Disposable);
        RefreshLocalCommand = new ReactiveCommand(RefreshLocalImpl).DisposeItWith(Disposable);

        LocalRenameCommand = CanRenameLocal
            .ToReactiveCommand<BrowserNode>(LocalRenameImpl, awaitOperation: AwaitOperation.Drop)
            .DisposeItWith(Disposable);
        RemoteRenameCommand = CanRenameRemote
            .ToReactiveCommand<BrowserNode>(
                async (node, ct) => await RemoteRenameImpl(node, ct),
                awaitOperation: AwaitOperation.Drop
            )
            .DisposeItWith(Disposable);

        // TODO: Upload incorrectly processes cancellation (prob. something wrong at Asv.Mavlink)
        UploadCommand = CanUpload
            .ToReactiveCommand<BrowserNode>(async (node, ct) => await UploadImpl(node, ct))
            .DisposeItWith(Disposable);
        DownloadCommand = CanDownload
            .ToReactiveCommand<BrowserNode>(async (node, ct) => await DownloadImpl(node, ct))
            .DisposeItWith(Disposable);
        BurstDownloadCommand = CanDownload
            .ToReactiveCommand<BrowserNode>(async (node, ct) => await BurstDownloadImpl(node, ct))
            .DisposeItWith(Disposable);

        CancelTransferCommand = new ReactiveCommand(_ => _transfer.TryCancel()).DisposeItWith(
            Disposable
        );

        CreateRemoteFolderCommand = new ReactiveCommand(
            async (_, ct) => await CreateRemoteFolderImpl(ct),
            awaitOperation: AwaitOperation.Drop
        ).DisposeItWith(Disposable);
        CreateLocalFolderCommand = new ReactiveCommand(
            CreateLocalFolderImpl,
            awaitOperation: AwaitOperation.Drop
        ).DisposeItWith(Disposable);
        RemoveLocalItemCommand = CanRemoveLocal
            .ToReactiveCommand<BrowserNode>(
                RemoveLocalItemImpl,
                awaitOperation: AwaitOperation.Drop
            )
            .DisposeItWith(Disposable);
        RemoveRemoteItemCommand = CanRemoveRemote
            .ToReactiveCommand<BrowserNode>(
                async (node, ct) => await RemoveRemoteItemImpl(node, ct),
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

        RefreshRemoteCommand.Execute(Unit.Default);
        RefreshLocalCommand.Execute(Unit.Default);
    }

    private async Task UploadImpl(BrowserNode item, CancellationToken ct)
    {
        if (_ftpService is null)
        {
            return;
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
            string remoteDirectory;
            if (RemoteSelectedItem.Value != null)
            {
                remoteDirectory = RemoteSelectedItem.Value.Base.HasChildren
                    ? RemoteSelectedItem.Value.Base.Path
                        + $"{LocalSelectedItem.Value?.Base.Name ?? BrowserNamingPolicy.BlankName}"
                    : RemoteSelectedItem.Value.Base.Path[
                        ..RemoteSelectedItem.Value.Base.Path.LastIndexOf(
                            MavlinkFtpHelper.DirectorySeparator
                        )
                    ]
                        + $"{MavlinkFtpHelper.DirectorySeparator}"
                        + $"{LocalSelectedItem.Value?.Base.Name ?? BrowserNamingPolicy.BlankName}";
            }
            else
            {
                remoteDirectory =
                    $"{MavlinkFtpHelper.DirectorySeparator}"
                    + $"{LocalSelectedItem.Value?.Base.Name ?? BrowserNamingPolicy.BlankName}";
            }

            var token = _transfer.Begin(ct);

            try
            {
                switch (item.Base.FtpEntryType)
                {
                    case FtpEntryType.File:
                        await _ftpService.UploadFileAsync(
                            item.Base.Path,
                            remoteDirectory,
                            token,
                            new Progress<double>(i =>
                            {
                                if (!Progress.IsCompletedOrDisposed)
                                {
                                    Progress.OnNext(i);
                                }
                            })
                        );
                        break;
                    case FtpEntryType.Directory:
                        await _ftpService.UploadDirectoryAsync(
                            item.Base.Path,
                            remoteDirectory,
                            token,
                            new Progress<double>(i =>
                            {
                                if (!Progress.IsCompletedOrDisposed)
                                {
                                    Progress.OnNext(i);
                                }
                            })
                        );
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(item));
                }
            }
            catch (OperationCanceledException)
            {
                Logger.LogWarning("Upload {Path} cancelled by user", item.Base.Path);
            }
            finally
            {
                _transfer.Complete();
            }
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
            var token = _transfer.Begin(ct);
            try
            {
                switch (item.Base.FtpEntryType)
                {
                    case FtpEntryType.File:
                        await _ftpService.DownloadFileAsync(
                            item.Base.Path,
                            localDirectory,
                            ct: token,
                            progress: new Progress<double>(i =>
                            {
                                if (!Progress.IsCompletedOrDisposed)
                                {
                                    Progress.OnNext(i);
                                }
                            })
                        );
                        break;
                    case FtpEntryType.Directory:
                        await _ftpService.DownloadDirectoryAsync(
                            item.Base.Path,
                            localDirectory,
                            ct: token,
                            progress: new Progress<double>(i =>
                            {
                                if (!Progress.IsCompletedOrDisposed)
                                {
                                    Progress.OnNext(i);
                                }
                            })
                        );
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(item));
                }
            }
            catch (OperationCanceledException)
            {
                Logger.LogWarning("Download {Path} cancelled by user", item.Base.Path);
            }
            finally
            {
                _transfer.Complete();
            }
        }
    }

    private async ValueTask BurstDownloadImpl(BrowserNode item, CancellationToken ct)
    {
        if (_ftpService is null)
        {
            return;
        }

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
            var size = viewModel.PacketSize.Value ?? MavlinkFtpHelper.MaxDataSize;
            var token = _transfer.Begin(ct);
            try
            {
                switch (item.Base.FtpEntryType)
                {
                    case FtpEntryType.File:
                        await _ftpService.BurstDownloadFileAsync(
                            item.Base.Path,
                            localDirectory,
                            size,
                            token,
                            new Progress<double>(i =>
                            {
                                if (!Progress.IsCompletedOrDisposed)
                                {
                                    Progress.OnNext(i);
                                }
                            })
                        );
                        break;
                    case FtpEntryType.Directory:
                        await _ftpService.BurstDownloadDirectoryAsync(
                            item.Base.Path,
                            localDirectory,
                            size,
                            token,
                            new Progress<double>(i =>
                            {
                                if (!Progress.IsCompletedOrDisposed)
                                {
                                    Progress.OnNext(i);
                                }
                            })
                        );
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(item));
                }
            }
            catch (OperationCanceledException)
            {
                Logger.LogWarning("Burst-Download {Path} cancelled by user", item.Base.Path);
            }
            finally
            {
                _transfer.Complete();
            }
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
        if (_ftpService is null)
        {
            return;
        }

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
                await _ftpService.RemoveDirectoryAsync(item.Base.Path, true, ct);
                break;
            case { FtpEntryType: FtpEntryType.File }:
                await _ftpService.RemoveFileAsync(item.Base.Path, ct);
                break;
        }
    }

    private async Task CreateRemoteFolderImpl(CancellationToken ct)
    {
        if (_ftpService is null)
        {
            return;
        }

        var path = RemoteSelectedItem.Value switch
        {
            { Base.FtpEntryType: FtpEntryType.Directory } dir => dir.Base.Path,
            { Base.Path: var p } => p[..p.LastIndexOf(MavlinkFtpHelper.DirectorySeparator)],
            _ => $"{MavlinkFtpHelper.DirectorySeparator}",
        };

        await _ftpService.CreateDirectoryAsync(path, ct);
    }

    private ValueTask CreateLocalFolderImpl(Unit arg, CancellationToken ct)
    {
        var path = LocalSelectedItem.Value switch
        {
            { Base.FtpEntryType: FtpEntryType.Directory } dir => dir.Base.Path,
            { Base.Path: var p } => Path.GetDirectoryName(p) ?? _localRootPath,
            _ => _localRootPath,
        };

        LocalFilesMixin.CreateDirectory(path, Logger);

        return ValueTask.CompletedTask;
    }

    private async Task RefreshRemoteImpl(CancellationToken ct)
    {
        if (_ftpService is null)
        {
            return;
        }

        var items = await _ftpService.Refresh(ct);

        var toRemove = _rawRemoteEntries
            .Where(oldItem =>
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
            )
            .ToList();
        foreach (var item in toRemove)
        {
            _rawRemoteEntries.Remove(item);
        }

        var toAdd = items
            .Where(newItem =>
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
            )
            .ToList();
        _rawRemoteEntries.AddRange(toAdd);
    }

    private ValueTask RefreshLocalImpl(Unit arg, CancellationToken ct)
    {
        var newItems = LocalFilesMixin.LoadBrowserItems(
            _localRootPath,
            _localRootPath,
            loggerFactory: _loggerFactory
        );

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

    private ValueTask LocalRenameImpl(BrowserNode? node, CancellationToken ct)
    {
        if (node?.Base is not BrowserItemViewModel item)
        {
            return ValueTask.CompletedTask;
        }

        item.EditedName.Value = item.Name ?? string.Empty;
        item.EditMode = true;

        LocalSelectedItem.OnNext(node);

        return ValueTask.CompletedTask;
    }

    private ValueTask RemoteRenameImpl(BrowserNode? node, CancellationToken ct)
    {
        if (_ftpService is null || node?.Base is not BrowserItemViewModel item)
        {
            return ValueTask.CompletedTask;
        }

        item.EditedName.Value = item.Name;
        item.EditMode = true;

        RemoteSelectedItem.OnNext(node);

        return ValueTask.CompletedTask;
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
        if (_ftpService is null)
        {
            return;
        }
        if (item.Base is not FileItemViewModel fileItem)
        {
            return;
        }

        var crc32 = await _ftpService.CalculateCrc32Async(fileItem.Path, ct);
        fileItem.Crc32 = crc32;
        fileItem.Crc32Status = Crc32Status.Default;
    }

    private async ValueTask CompareSelectedItemsImpl(CancellationToken ct)
    {
        if (_ftpService is null)
        {
            return;
        }

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
            remoteCrc32 = await _ftpService.CalculateCrc32Async(remoteFileItem.Path, ct);
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

    #region Helpers

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

    private IBrowserItemViewModel EntryToBrowserItem(string key, IFtpEntry entry)
    {
        const char sep = MavlinkFtpHelper.DirectorySeparator;

        switch (entry.Type)
        {
            case FtpEntryType.Directory:
            {
                var dirPath = BrowserPathRules.Normalize(key, true, sep);
                return new DirectoryItemViewModel(
                    PathHelper.EncodePathToId(dirPath),
                    BrowserPathRules.ParentDirOf(dirPath, sep),
                    dirPath,
                    entry.Name,
                    EntityType.Remote,
                    _ftpService,
                    _loggerFactory
                );
            }
            case FtpEntryType.File:
            {
                var filePath = BrowserPathRules.Normalize(key, false, sep);
                return new FileItemViewModel(
                    PathHelper.EncodePathToId(filePath),
                    BrowserPathRules.ParentDirOf(filePath, sep),
                    filePath,
                    entry.Name,
                    ((FtpFile)entry).Size,
                    EntityType.Remote,
                    _ftpService,
                    _loggerFactory
                );
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(entry));
        }
    }

    #endregion

    public override ValueTask<IRoutable> Navigate(NavigationId id)
    {
        return ValueTask.FromResult<IRoutable>(this);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        yield return LocalSearchText;
        yield return RemoteSearchText;

        foreach (var item in _localItems)
        {
            yield return item;
        }

        foreach (var item in _remoteItems)
        {
            yield return item;
        }
    }

    protected override void AfterLoadExtensions() { }

    public override IExportInfo Source => SystemModule.Instance;

    #region Dispose

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _watcher.Created -= _createdHandler;
            _watcher.Deleted -= _deletedHandler;
            _watcher.Renamed -= _renamedHandler;
            _watcher.Changed -= _changedHandler;

            _localItems.RemoveAll();
            _remoteItems.RemoveAll();
        }

        base.Dispose(disposing);
    }

    #endregion

    protected override void AfterDeviceInitialized(
        IClientDevice device,
        CancellationToken onDisconnectedToken
    )
    {
        IsDeviceInitialized = true;
        Title = $"{RS.FileBrowserViewModel_Title}[{device.Id}]";
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

            IsDeviceInitialized = false;
            _ftpService = null;
        });

        RefreshRemoteCommand.Execute(Unit.Default);
    }
}
