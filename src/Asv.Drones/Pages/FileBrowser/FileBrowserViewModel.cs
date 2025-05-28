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
    private readonly ILogger<FileBrowserViewModel> _log;
    private readonly INavigationService _navigation;
    private readonly string _localRootPath;

    private readonly ObservableList<IBrowserItem> _localItems;
    private readonly ObservableList<IBrowserItem> _remoteItems;

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

        _localItems =
        [
            new DirectoryItem("0", string.Empty, "/", "test"),
            new DirectoryItem("1", "0", "/", "test"),
            new DirectoryItem("2", "0", "/", "test"),
            new DirectoryItem("0_1", string.Empty, "/", "test"),
            new DirectoryItem("0_2", string.Empty, "/", "test"),
        ];
        _remoteItems =
        [
            new DirectoryItem("0", string.Empty, "/", "test"),
            new DirectoryItem("1", "0", "/", "test"),
            new DirectoryItem("2", "0", "/", "test"),
            new DirectoryItem("0_1", string.Empty, "/", "test"),
            new DirectoryItem("0_2", string.Empty, "/", "test"),
        ];

        LocalItemsView = new BrowserTree(_localItems, _localRootPath);
        RemoteItemsView = new BrowserTree(
            _remoteItems,
            MavlinkFtpHelper.DirectorySeparator.ToString()
        );
    }

    [ImportingConstructor]
    public FileBrowserViewModel(
        ICommandService cmd,
        IDeviceManager devices,
        IDialogService dialogService,
        IAppPath appPath,
        ILoggerFactory log,
        INavigationService navigation
    )
        : base(PageId, devices, cmd)
    {
        _localRootPath = appPath.UserDataFolder;
        _dialogService = dialogService;
        _log = log.CreateLogger<FileBrowserViewModel>();
        _navigation = navigation;
        _yesNoDialog = dialogService.GetDialogPrefab<YesOrNoDialogPrefab>();

        _localItems = [];
        _remoteItems = [];
        _localItems.DisposeRemovedItems();
        _remoteItems.DisposeRemovedItems();

        LocalItemsView = new BrowserTree(_localItems, _localRootPath);
        RemoteItemsView = new BrowserTree(
            _remoteItems,
            MavlinkFtpHelper.DirectorySeparator.ToString()
        );

        LocalSearchText = new BindableReactiveProperty<string>();
        RemoteSearchText = new BindableReactiveProperty<string>();
        LocalSelectedItem = new BindableReactiveProperty<BrowserNode?>(null);
        RemoteSelectedItem = new BindableReactiveProperty<BrowserNode?>(null);
        Progress = new BindableReactiveProperty<double>(0);
        IsDownloadPopupOpen = new BindableReactiveProperty<bool>(false);
    }

    private IFtpClient? Client { get; set; }
    private IFtpClientEx ClientEx { get; set; }
    public BrowserTree LocalItemsView { get; }
    public BrowserTree RemoteItemsView { get; }
    public BindableReactiveProperty<BrowserNode?> LocalSelectedItem { get; }
    public BindableReactiveProperty<BrowserNode?> RemoteSelectedItem { get; }
    public BindableReactiveProperty<string> LocalSearchText { get; }
    public BindableReactiveProperty<string> RemoteSearchText { get; }
    public BindableReactiveProperty<double> Progress { get; }
    public BindableReactiveProperty<bool> IsDownloadPopupOpen { get; }

    #region Commands

    public ReactiveCommand ShowDownloadPopupCommand { get; private set; }
    public ReactiveCommand UploadCommand { get; private set; }
    public ReactiveCommand DownloadCommand { get; private set; }
    public ReactiveCommand BurstDownloadCommand { get; private set; }
    public ReactiveCommand CreateRemoteFolderCommand { get; private set; }
    public ReactiveCommand CreateLocalFolderCommand { get; private set; }
    public ReactiveCommand RefreshRemoteCommand { get; private set; }
    public ReactiveCommand RefreshLocalCommand { get; private set; }
    public ReactiveCommand RemoveLocalItemCommand { get; private set; }
    public ReactiveCommand RemoveRemoteItemCommand { get; set; }
    public ReactiveCommand<BrowserNode> SetInEditModeCommand { get; private set; } // TODO: implement
    public ReactiveCommand ClearLocalSearchBoxCommand { get; private set; } // TODO: implement
    public ReactiveCommand ClearRemoteSearchBoxCommand { get; private set; } // TODO: implement
    public ReactiveCommand CompareSelectedItemsCommand { get; private set; } // TODO: implement
    public ReactiveCommand FindFileOnLocalCommand { get; private set; } // TODO: implement
    public ReactiveCommand CalculateLocalCrc32Command { get; private set; } // TODO: implement
    public ReactiveCommand CalculateRemoteCrc32Command { get; private set; } // TODO: implement

    private Observable<bool> CanUpload =>
        LocalSelectedItem.Select(x => x?.Base is { FtpEntryType: FtpEntryType.File });

    private Observable<bool> CanDownload =>
        RemoteSelectedItem.Select(x => x?.Base is { FtpEntryType: FtpEntryType.File });

    private Observable<bool> CanEdit =>
        LocalSelectedItem.Select(x => x?.Base is { IsInEditMode: false });

    private Observable<bool> CanFindFileOnLocal =>
        RemoteSelectedItem.Select(x => x?.Base is { IsInEditMode: false });

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

    #endregion

    #region Commands implementation

    private void InitCommands()
    {
        // Not implemented
        SetInEditModeCommand = CanEdit.ToReactiveCommand<BrowserNode>(SetEditModeImpl);
        ClearLocalSearchBoxCommand = new ReactiveCommand(_ => LocalSearchText.OnNext(string.Empty));
        ClearRemoteSearchBoxCommand = new ReactiveCommand(_ =>
            RemoteSearchText.OnNext(string.Empty)
        );
        FindFileOnLocalCommand = CanFindFileOnLocal.ToReactiveCommand(_ => FindFileOnLocalImpl());
        CompareSelectedItemsCommand = CanCompareSelectedItems.ToReactiveCommand(_ =>
            CompareSelectedItemsImpl()
        );
        CalculateLocalCrc32Command = CanCalculateLocalCrc32.ToReactiveCommand(_ =>
            CalculateLocalCrc32Impl()
        );
        CalculateRemoteCrc32Command = CanCalculateRemoteCrc32.ToReactiveCommand(_ =>
            CalculateRemoteCrc32Impl()
        );

        // Implemented
        RefreshRemoteCommand = new ReactiveCommand(async (_, ct) => await RefreshRemoteImpl(ct));
        RefreshLocalCommand = new ReactiveCommand(async (_, _) => await RefreshLocalImpl());

        UploadCommand = CanUpload.ToReactiveCommand(_ => SafeUpload());
        DownloadCommand = CanDownload.ToReactiveCommand(_ => SafeDownload());
        BurstDownloadCommand = CanDownload.ToReactiveCommand(_ => SafeBurstDownload());
        CreateRemoteFolderCommand = new ReactiveCommand(
            async (_, ct) =>
            {
                await CreateRemoteFolderImpl(ct);
                await RefreshRemoteImpl(ct);
            },
            awaitOperation: AwaitOperation.Drop
        );
        CreateLocalFolderCommand = new ReactiveCommand(
            async (_, _) =>
            {
                await CreateLocalFolderImpl();
                await RefreshLocalImpl();
            },
            awaitOperation: AwaitOperation.Drop
        );
        RemoveLocalItemCommand = new ReactiveCommand(
            async (_, _) =>
            {
                await RemoveLocalItemImpl();
                await RefreshLocalImpl();
            },
            awaitOperation: AwaitOperation.Drop
        );
        RemoveRemoteItemCommand = new ReactiveCommand(
            async (_, ct) =>
            {
                await RemoveRemoteItemImpl(ct);
                await RefreshRemoteImpl(ct);
            },
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

        RefreshRemoteCommand.SubscribeOnUIThreadDispatcher();
        RefreshLocalCommand.SubscribeOnUIThreadDispatcher();

        RefreshRemoteCommand.Execute(Unit.Default);
        RefreshLocalCommand.Execute(Unit.Default);
    }

    #region Safe

    private CancellationTokenSource _uploadCts = new();
    private CancellationTokenSource _downloadCts = new();
    private CancellationTokenSource _burstDownloadCts = new();

    private void SafeUpload()
    {
        _uploadCts.Cancel();
        _uploadCts = new CancellationTokenSource();

        UploadImpl(_uploadCts.Token)
            .ContinueWith(_ => RefreshRemoteImpl(_uploadCts.Token))
            .SafeFireAndForget(e =>
            {
                switch (e)
                {
                    case OperationCanceledException:
                        _log.LogWarning("Upload was canceled.");
                        break;
                    case FtpNackException:
                        _log.LogError(e, "Server returned NACK.");
                        break;
                    case not null:
                        _log.LogError(e, "Unexpected error during upload.");
                        break;
                }
            });
    }

    private void SafeDownload()
    {
        _downloadCts.Cancel();
        _downloadCts = new CancellationTokenSource();

        DownloadImpl(_downloadCts.Token)
            .ContinueWith(_ => RefreshLocalImpl())
            .SafeFireAndForget(e =>
            {
                switch (e)
                {
                    case OperationCanceledException:
                        _log.LogWarning("Download was canceled.");
                        break;
                    case FtpNackException:
                        _log.LogError(e, "Server returned NACK.");
                        break;
                    case not null:
                        _log.LogError(e, "Unexpected error during download.");
                        break;
                }
            });
    }

    private void SafeBurstDownload()
    {
        _burstDownloadCts.Cancel();
        _burstDownloadCts = new CancellationTokenSource();

        BurstDownloadImpl(_burstDownloadCts.Token)
            .ContinueWith(_ => RefreshLocalImpl())
            .SafeFireAndForget(e =>
            {
                switch (e)
                {
                    case OperationCanceledException:
                        _log.LogWarning("Burst download was canceled.");
                        break;
                    case FtpNackException:
                        _log.LogError(e, "Server returned NACK.");
                        break;
                    case not null:
                        _log.LogError(e, "Unexpected error during burst download.");
                        break;
                }
            });
    }

    #endregion

    private async Task UploadImpl(CancellationToken ct)
    {
        var item = LocalSelectedItem.Value;
        if (item is null)
        {
            return;
        }

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
                new Progress<double>(i => Progress.OnNext(i)),
                ct
            );
        }
    }

    private async Task DownloadImpl(CancellationToken ct)
    {
        var item = RemoteSelectedItem.Value;
        if (item == null)
        {
            return;
        }

        var path = _localRootPath;
        if (LocalSelectedItem.Value != null)
        {
            path = Path.Combine(
                LocalSelectedItem.Value.Base.HasChildren
                    ? LocalSelectedItem.Value.Base.Path
                    : LocalSelectedItem.Value.Base.Path[
                        ..LocalSelectedItem.Value.Base.Path.LastIndexOf(Path.DirectorySeparatorChar)
                    ],
                RemoteSelectedItem.Value!.Base.Header!
            );
        }
        else
        {
            path = Path.Combine(path, RemoteSelectedItem.Value!.Base.Header!);
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
            await using MemoryStream stream = new();

            await ClientEx.DownloadFile(
                item.Base.Path,
                stream,
                new Progress<double>(i => Progress.OnNext(i)),
                cancel: ct
            );
            await File.WriteAllBytesAsync(path, stream.ToArray(), ct);
            _log.LogInformation(
                "File downloaded successfully: {Header}",
                RemoteSelectedItem.Value?.Base.Header
            );
        }
    }

    private async Task BurstDownloadImpl(CancellationToken ct)
    {
        var item = RemoteSelectedItem.Value;
        if (item == null)
        {
            return;
        }

        var path = _localRootPath;
        if (LocalSelectedItem.Value != null)
        {
            path = Path.Combine(
                LocalSelectedItem.Value.Base.HasChildren
                    ? LocalSelectedItem.Value.Base.Path
                    : LocalSelectedItem.Value.Base.Path[
                        ..LocalSelectedItem.Value.Base.Path.LastIndexOf(Path.DirectorySeparatorChar)
                    ],
                RemoteSelectedItem.Value!.Base.Header!
            );
        }
        else
        {
            path = Path.Combine(path, RemoteSelectedItem.Value!.Base.Header!);
        }

        using var viewModel = new BurstDownloadDialogViewModel("burst.dialog");
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
            await using MemoryStream stream = new();

            await ClientEx.BurstDownloadFile(
                item.Base.Path,
                stream,
                new Progress<double>(i => Progress.OnNext(i)),
                viewModel.DialogResult,
                ct
            );
            await File.WriteAllBytesAsync(path, stream.ToArray(), ct);
            _log.LogInformation(
                "File downloaded successfully: {Header}",
                RemoteSelectedItem.Value?.Base.Header
            );

            await RefreshLocalImpl();
        }
    }

    private async Task RemoveLocalItemImpl()
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

        if (LocalSelectedItem.Value?.Base is { FtpEntryType: FtpEntryType.Directory })
        {
            Directory.Delete(LocalSelectedItem.Value.Base.Path, true);
        }

        if (LocalSelectedItem.Value?.Base is { FtpEntryType: FtpEntryType.File })
        {
            File.Delete(LocalSelectedItem.Value.Base.Path);
        }
    }

    private async Task RemoveRemoteItemImpl(CancellationToken ct)
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

        if (RemoteSelectedItem.Value?.Base is { FtpEntryType: FtpEntryType.Directory })
        {
            await RemoveDirectoryRecursive(RemoteSelectedItem.Value.Base.Path);
        }

        if (RemoteSelectedItem.Value?.Base is { FtpEntryType: FtpEntryType.File })
        {
            await ClientEx.Base.RemoveFile(RemoteSelectedItem.Value.Base.Path, ct);
        }

        return;

        async Task RemoveDirectoryRecursive(string directoryPath)
        {
            var itemsInDir = ClientEx
                .Entries.Where(x => x.Value.ParentPath == directoryPath)
                .ToList();

            foreach (var item in itemsInDir)
            {
                switch (item.Value.Type)
                {
                    case FtpEntryType.Directory:
                        await RemoveDirectoryRecursive(item.Key);
                        break;
                    case FtpEntryType.File:
                        await ClientEx.Base.RemoveFile(item.Key, ct);
                        break;
                    default:
                        _log.LogError("Unknown FTP entry type: ({type})", item.Value.Type);
                        break;
                }
            }

            await ClientEx.Base.RemoveDirectory(directoryPath, ct);
        }
    }

    private async Task CreateRemoteFolderImpl(CancellationToken ct)
    {
        var folderNumber = 1;
        while (true)
        {
            var name = $"Folder{folderNumber}{MavlinkFtpHelper.DirectorySeparator}";
            string path;
            if (RemoteSelectedItem.Value != null)
            {
                path =
                    RemoteSelectedItem.Value!.Base.FtpEntryType == FtpEntryType.Directory
                        ? Path.Combine(RemoteSelectedItem.Value.Base.Path, name)
                        : Path.Combine(
                            RemoteSelectedItem.Value.Base.Path[
                                ..RemoteSelectedItem.Value.Base.Path.LastIndexOf(
                                    MavlinkFtpHelper.DirectorySeparator
                                )
                            ],
                            name
                        );
            }
            else
            {
                path = $"{MavlinkFtpHelper.DirectorySeparator}{name}";
            }

            ClientEx.Entries.FirstOrDefault(x => x.Key == path).Deconstruct(out var k, out _);
            if (!string.IsNullOrEmpty(k))
            {
                folderNumber++;
                continue;
            }

            await ClientEx.Base.CreateDirectory(path, ct);

            break;
        }
    }

    private Task CreateLocalFolderImpl()
    {
        var folderNumber = 1;
        while (true)
        {
            var name = $"Folder{folderNumber}";
            string path;
            if (LocalSelectedItem.Value != null)
            {
                path =
                    LocalSelectedItem.Value.Base.FtpEntryType == FtpEntryType.Directory
                        ? Path.Combine(LocalSelectedItem.Value.Base.Path, name)
                        : Path.Combine(
                            LocalSelectedItem.Value.Base.Path[
                                ..LocalSelectedItem.Value.Base.Path.LastIndexOf(
                                    Path.DirectorySeparatorChar
                                )
                            ],
                            name
                        );
            }
            else
            {
                path = Path.Combine(_localRootPath, name);
            }

            if (Directory.Exists(path))
            {
                folderNumber++;
                continue;
            }

            Directory.CreateDirectory(path);

            break;
        }

        return Task.CompletedTask;
    }

    private async Task RefreshRemoteImpl(CancellationToken ct)
    {
        await ClientEx.Refresh(MavlinkFtpHelper.DirectorySeparator.ToString(), cancel: ct);
        var newItems = LoadRemoteItems();

        var toRemove = _remoteItems.Where(rs => newItems.All(n => n.Path != rs.Path)).ToList();
        foreach (var item in toRemove)
        {
            _remoteItems.Remove(item);
        }

        var toAdd = newItems.Where(n => _remoteItems.All(rs => rs.Path != n.Path)).ToList();
        _remoteItems.AddRange(toAdd);
    }

    private Task RefreshLocalImpl()
    {
        var newItems = LoadLocalItems();

        var toRemove = _localItems.Where(ls => newItems.All(n => n.Path != ls.Path)).ToList();
        foreach (var item in toRemove)
        {
            _localItems.Remove(item);
        }

        var toAdd = newItems.Where(n => _localItems.All(ls => ls.Path != n.Path)).ToList();
        _localItems.AddRange(toAdd);

        return Task.CompletedTask;
    }

    #endregion

    #region Load items

    private ObservableList<IBrowserItem> LoadLocalItems()
    {
        var items = new ObservableList<IBrowserItem>();
        ProcessDirectory(_localRootPath, items);
        return items;
    }

    private void ProcessDirectory(string directoryPath, ObservableList<IBrowserItem> items)
    {
        var directories = Directory.EnumerateDirectories(directoryPath).ToList();
        var files = Directory.EnumerateFiles(directoryPath).ToList();

        Parallel.ForEach(
            directories,
            dir =>
            {
                var relativeDir = Path.GetRelativePath(_localRootPath, dir);
                var id = NavigationId.NormalizeTypeId(relativeDir);
                var parentPath = Directory.GetParent(dir)?.FullName ?? _localRootPath;
                var name = new DirectoryInfo(dir).Name;

                lock (items)
                {
                    items.Add(new DirectoryItem(id, parentPath, dir, name));
                }

                ProcessDirectory(dir, items);
            }
        );

        Parallel.ForEach(
            files,
            file =>
            {
                var relativeFile = Path.GetRelativePath(_localRootPath, file);
                var id = NavigationId.NormalizeTypeId(relativeFile);
                var parentPath = Directory.GetParent(file)?.FullName ?? _localRootPath;
                var fileInfo = new FileInfo(file);

                lock (items)
                {
                    items.Add(new FileItem(id, parentPath, file, fileInfo.Name, fileInfo.Length));
                }
            }
        );
    }

    private ObservableList<IBrowserItem> LoadRemoteItems()
    {
        var items = new ObservableList<IBrowserItem>();

        ClientEx.Entries.ForEach(e =>
        {
            if (e.Value.Path == MavlinkFtpHelper.DirectorySeparator.ToString())
            {
                var root = new DirectoryItem(
                    "_",
                    string.Empty,
                    MavlinkFtpHelper.DirectorySeparator.ToString(),
                    "_"
                );
                items.Add(root);
                return;
            }

            var item = e.Value.Type switch
            {
                FtpEntryType.Directory => new DirectoryItem(
                    NavigationId.NormalizeTypeId(e.Value.Path),
                    e.Value.ParentPath,
                    e.Key,
                    e.Value.Name
                ),
                FtpEntryType.File => new FileItem(
                    NavigationId.NormalizeTypeId(e.Value.Path),
                    e.Value.ParentPath,
                    e.Key,
                    e.Value.Name,
                    ((FtpFile)e.Value).Size
                ),
                _ => new BrowserItem(
                    NavigationId.NormalizeTypeId(e.Value.Path),
                    e.Value.ParentPath,
                    e.Key
                ),
            };

            items.Add(item);
        });

        return items;
    }

    #endregion

    private void CalculateLocalCrc32Impl()
    {
        throw new NotImplementedException();
    }

    private void CalculateRemoteCrc32Impl()
    {
        throw new NotImplementedException();
    }

    private void CompareSelectedItemsImpl()
    {
        throw new NotImplementedException();
    }

    private void FindFileOnLocalImpl()
    {
        throw new NotImplementedException();
    }

    private void SetEditModeImpl(BrowserNode item)
    {
        throw new NotImplementedException();
    }

    private static string Crc32ToHex(uint crc32) => crc32.ToString("X8");

    public override ValueTask<IRoutable> Navigate(NavigationId id)
    {
        return ValueTask.FromResult<IRoutable>(this);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    protected override void AfterLoadExtensions() { }

    protected override void AfterDeviceInitialized(IClientDevice device)
    {
        Title = $"Browser[{device.Id}]";
        Client = device.GetMicroservice<IFtpClient>();
        ArgumentNullException.ThrowIfNull(Client);
        ClientEx = device.GetMicroservice<IFtpClientEx>() ?? new FtpClientEx(Client);
        InitCommands();
    }

    public override IExportInfo Source => SystemModule.Instance;

    #region Dispose

    private IDisposable _sub1;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sub1.Dispose();

            _uploadCts.Cancel();
            _uploadCts.Dispose();
            _downloadCts.Cancel();
            _downloadCts.Dispose();
            _burstDownloadCts.Cancel();
            _burstDownloadCts.Dispose();

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
            SetInEditModeCommand.Dispose();
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

            LocalItemsView.Dispose();
            RemoteItemsView.Dispose();

            _localItems.RemoveRange(0, _localItems.Count);
            _remoteItems.RemoveRange(0, _remoteItems.Count);
        }

        base.Dispose(disposing);
    }

    #endregion
}
