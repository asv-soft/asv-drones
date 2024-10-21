using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Composition;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Asv.Common;
using Asv.Drones.Gui.Api;
using Asv.Mavlink;
using Avalonia.Threading;
using DynamicData;
using DynamicData.Binding;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using MavlinkHelper = Asv.Drones.Gui.Api.MavlinkHelper;

namespace Asv.Drones.Gui;

[ExportShellPage(WellKnownUri.ShellPageVehicleFileBrowser)]
public class VehicleFileBrowserViewModel : ShellPage
{
    private readonly ILogService _log;
    private readonly IMavlinkDevicesService _svc;
    private readonly string _localRootPath;

    private readonly SourceCache<FileSystemItemViewModel, string> _localSource; 
    private readonly SourceCache<FileSystemItemViewModel, string> _remoteSource;
    
    private readonly ReadOnlyObservableCollection<FileSystemItemViewModel> _filteredLocalItems;
    private readonly ReadOnlyObservableCollection<FileSystemItemViewModel> _filteredRemoteItems;

    public VehicleFileBrowserViewModel() : base(WellKnownUri.UndefinedUri)
    {
        DesignTime.ThrowIfNotDesignMode();
    }
    
    [ImportingConstructor]
    public VehicleFileBrowserViewModel(IMavlinkDevicesService svc, IApplicationHost appService, ILogService log) 
        : base(WellKnownUri.ShellPageVehicleFileBrowser)
    {
        _log = log;
        _svc = svc;
        _localRootPath = appService.Paths.AppDataFolder;

        _localSource = new SourceCache<FileSystemItemViewModel, string>(x => x.Path)
            .DisposeItWith(Disposable);
        _remoteSource = new SourceCache<FileSystemItemViewModel, string>(x => x.Path)
            .DisposeItWith(Disposable);
        
        var localFilterPipe = new Subject<Func<FileSystemItemViewModel, bool>>()
            .DisposeItWith(Disposable);
        localFilterPipe.OnNext(_ => true);
        this.WhenValueChanged(x => x.LocalSearchText)
            .Throttle(TimeSpan.FromMilliseconds(500), RxApp.MainThreadScheduler)
            .Subscribe(search =>
            {
                localFilterPipe.OnNext(item => MatchesSearch(item, search));
                var match = FindFirstMatchingItem(_localSource.Items, search);
                if (match == null) return;
                ExpandParents(match);
                LocalSelectedItem = match;
            })
            .DisposeItWith(Disposable);
        _localSource.Connect()
            .Filter(localFilterPipe)
            .SortBy(x => !x.IsDirectory)
            .Bind(out _filteredLocalItems)
            .Subscribe();
        
        var remoteFilterPipe = new Subject<Func<FileSystemItemViewModel, bool>>()
            .DisposeItWith(Disposable);
        remoteFilterPipe.OnNext(_ => true);
        this.WhenValueChanged(x => x.RemoteSearchText)
            .Throttle(TimeSpan.FromMilliseconds(500), RxApp.MainThreadScheduler)
            .Subscribe(search =>
            {
                remoteFilterPipe.OnNext(item => MatchesSearch(item, search));
                var match = FindFirstMatchingItem(_remoteSource.Items, search);
                if (match == null) return;
                ExpandParents(match);
                RemoteSelectedItem = match;
            })
            .DisposeItWith(Disposable);
        _remoteSource.Connect()
            .Filter(remoteFilterPipe)
            .SortBy(x => !x.IsDirectory)
            .Bind(out _filteredRemoteItems)
            .Subscribe();
        
        _localSource.Connect()
            .AutoRefreshOnObservable(x => x.WhenAnyValue(item => item.IsSelected))
            .Subscribe(_ =>
            {
                var selectedItem = _localSource.Items.FirstOrDefault(item => item.IsSelected);
                if (selectedItem != null && LocalSelectedItem != selectedItem) LocalSelectedItem = selectedItem;
            })
            .DisposeItWith(Disposable);
        _remoteSource.Connect()
            .AutoRefreshOnObservable(x => x.WhenAnyValue(item => item.IsSelected))
            .Subscribe(_ =>
            {
                var selectedItem = _remoteSource.Items.FirstOrDefault(item => item.IsSelected);
                if (selectedItem != null && RemoteSelectedItem != selectedItem) RemoteSelectedItem = selectedItem;
            })
            .DisposeItWith(Disposable);
    }
    
    public ReactiveCommand<Unit, Unit> UploadCommand { get; set; }
    public ReactiveCommand<Unit, Unit> DownloadCommand { get; set; }
    public ReactiveCommand<Unit, Unit> CreateRemoteFolderCommand { get; set; }
    public ReactiveCommand<Unit, Unit> CreateLocalFolderCommand { get; set; }
    public ReactiveCommand<Unit, Unit> RefreshRemoteCommand { get; set; }
    public ReactiveCommand<Unit, Unit> RefreshLocalCommand { get; set; }
    public ReactiveCommand<Unit, Unit> RemoveLocalItemCommand { get; set; }
    public ReactiveCommand<Unit, Unit> RemoveRemoteItemCommand { get; set; }
    public ReactiveCommand<Unit, Unit> SetInEditModeCommand { get; set; }
    public ReactiveCommand<Unit, Unit> ClearLocalSearchBoxCommand { get; set; }
    public ReactiveCommand<Unit, Unit> ClearRemoteSearchBoxCommand { get; set; }
    
    private IObservable<bool> CanUpload => 
        this.WhenValueChanged(x => LocalSelectedItem)
            .Select(x => x is { IsDirectory: false });
    private IObservable<bool> CanDownload => 
        this.WhenValueChanged(x => RemoteSelectedItem)
            .Select(x => x is { IsDirectory: false });
    private IObservable<bool> CanEdit => 
        this.WhenValueChanged(x => LocalSelectedItem)
            .Select(x => x is { IsInEditMode: false });

    public ReadOnlyObservableCollection<FileSystemItemViewModel> FilteredLocalItems => _filteredLocalItems;
    public ReadOnlyObservableCollection<FileSystemItemViewModel> FilteredRemoteItems => _filteredRemoteItems;
    [Reactive] public FileSystemItemViewModel? LocalSelectedItem { get; set; }
    [Reactive] public FileSystemItemViewModel? RemoteSelectedItem { get; set; }
    [Reactive] public string LocalSearchText { get; set; }
    [Reactive] public string RemoteSearchText { get; set; }
    
    public override void SetArgs(NameValueCollection args)
    {
        base.SetArgs(args);
        if (ushort.TryParse(args["id"], out var id) == false) return;
        if (Enum.TryParse<DeviceClass>(args["class"], true, out var deviceClass) == false) return;
        Icon = MavlinkHelper.GetIcon(deviceClass);
        
        var vehicle = _svc.GetVehicleByFullId(id);
        if (vehicle == null) return; 
        
        Title = $"{vehicle.Class}: {vehicle.Name.Value}";
        
        var ftpClientEx = new FtpClientEx(vehicle.Ftp, TimeProvider.System);
        
        UploadCommand = ReactiveCommand.CreateFromTask(_ => UploadImpl(ftpClientEx), CanUpload);
        DownloadCommand = ReactiveCommand.CreateFromTask(_ => DownloadImpl(ftpClientEx), CanDownload);
        CreateRemoteFolderCommand = ReactiveCommand.CreateFromTask(_ => CreateRemoteFolderImpl(ftpClientEx));
        CreateLocalFolderCommand = ReactiveCommand.CreateFromTask(CreateLocalFolderImpl);
        RefreshRemoteCommand = ReactiveCommand.CreateFromTask(_ => RefreshRemoteImpl(ftpClientEx));
        RefreshLocalCommand = ReactiveCommand.CreateFromTask(RefreshLocalImpl);
        RemoveLocalItemCommand = ReactiveCommand.CreateFromTask(RemoveLocalItemImpl);
        RemoveRemoteItemCommand = ReactiveCommand.CreateFromTask(_ => RemoveRemoteItemImpl(ftpClientEx));
        SetInEditModeCommand = ReactiveCommand.CreateFromTask(_ => SetInEditModeImpl(LocalSelectedItem!), CanEdit);
        ClearLocalSearchBoxCommand = ReactiveCommand.Create(ClearLocalSearchBoxImpl);
        ClearRemoteSearchBoxCommand = ReactiveCommand.Create(ClearRemoteSearchBoxImpl);
            
        Task.Run(async () =>
        {
            await RefreshRemoteImpl(ftpClientEx);
        });
        
        LoadLocalItems(_localRootPath);
        LoadRemoteItems(ftpClientEx);
    }
    
    private async Task UploadImpl(FtpClientEx ftpClientEx)
    {
        var dialog = new ContentDialog
        {
            Title = RS.VehicleFileBrowserViewModel_UploadDialog_Title,
            PrimaryButtonText = RS.VehicleFileBrowserViewModel_UploadDialog_PrimaryButtonText
        };

        using var viewModel = new UploadFileDialogViewModel(_log, ftpClientEx, RemoteSelectedItem!, LocalSelectedItem);
        dialog.Content = viewModel;
        viewModel.ApplyDialog(dialog);
        await dialog.ShowAsync();

        await RefreshRemoteImpl(ftpClientEx);
    }

    private async Task DownloadImpl(FtpClientEx ftpClientEx)
    {
        var dialog = new ContentDialog
        {
            Title = RS.VehicleFileBrowserViewModel_DownloadDialog_Title,
            PrimaryButtonText = RS.VehicleFileBrowserViewModel_DownloadDialog_PrimaryButtonText
        };

        using var viewModel = new DownloadFileDialogViewModel(_log, ftpClientEx, _localRootPath, RemoteSelectedItem!, LocalSelectedItem);
        dialog.Content = viewModel;
        viewModel.ApplyDialog(dialog);
        await dialog.ShowAsync();
        
        await RefreshLocalImpl();
    }

    private void ClearLocalSearchBoxImpl() => LocalSearchText = string.Empty;
    
    private void ClearRemoteSearchBoxImpl() => RemoteSearchText = string.Empty;
    
    private async Task RemoveLocalItemImpl()
    {
        if (LocalSelectedItem is { IsDirectory: true }) Directory.Delete(LocalSelectedItem.Path, true);
        if (LocalSelectedItem is { IsFile: true }) File.Delete(LocalSelectedItem.Path);
        await RefreshLocalImpl();
    }
    
    private async Task RemoveRemoteItemImpl(FtpClientEx ftpClientEx)
    {
        if (RemoteSelectedItem is { IsDirectory: true }) await ftpClientEx.Base.RemoveDirectory(RemoteSelectedItem.Path);
        if (RemoteSelectedItem is { IsFile: true }) await ftpClientEx.Base.RemoveFile(RemoteSelectedItem.Path);
        await RefreshRemoteImpl(ftpClientEx);
    }

    private async Task SetInEditModeImpl(FileSystemItemViewModel item)
    {
        item.IsInEditMode = true;
        item.EditedName = item.Name;
        await CommitEdit(item);
    }

    private async Task CommitEdit(FileSystemItemViewModel item)
    {
        await Task.Run(() => { while (item.IsInEditMode) { } });
        if (!item.IsEditingSuccess) return;
        
        var oldPath = item.Path;
        var oldName = item.Name;
        var directoryPath = Path.GetDirectoryName(oldPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);
        var newPath = Path.Combine(directoryPath, item.EditedName);
        
        if (oldPath == newPath)
        {
            item.IsInEditMode = false;
            item.IsEditingSuccess = false;
        }
        
        try
        {
            if (item.IsFile) File.Move(oldPath, newPath);
            if (item.IsDirectory) Directory.Move(oldPath, newPath);

            item.Name = item.EditedName;
            item.Path = newPath;

            await RefreshLocalImpl();
            _log.Info(nameof(VehicleFileBrowserViewModel), $"File renamed from {oldName} to {item.Name}");

            item.IsInEditMode = false;
            item.IsEditingSuccess = false;
        }
        catch (Exception ex)
        {
            _log.Error(nameof(VehicleFileBrowserViewModel), $"Failed to rename file from {oldPath} to {newPath}", ex);
        }
    }

    private async Task CreateRemoteFolderImpl(FtpClientEx ftpClientEx)
    {
        var dir = "/Folder1/sub1";
        await ftpClientEx.Base.CreateDirectory(dir);
        await RefreshRemoteImpl(ftpClientEx);
    }

    private Task CreateLocalFolderImpl()
    {
        CreateFolder(1);
        RefreshLocalImpl();
        return Task.CompletedTask;

        void CreateFolder(int n)
        {
            while (true)
            {
                var name = $"Folder{n}";
                if (LocalSelectedItem != null)
                {
                    Directory.CreateDirectory(LocalSelectedItem.IsDirectory
                        ? Path.Combine(LocalSelectedItem.Path, name)
                        : Path.Combine(LocalSelectedItem.Path[..LocalSelectedItem.Path.LastIndexOf('\\')], name));
                }
                else
                {
                    var path = Path.Combine(_localRootPath, name);
                    if (Directory.Exists(path))
                    {
                        n += 1;
                        continue;
                    }

                    Directory.CreateDirectory(path);
                }
                break;
            }
        }
    }

    private async Task RefreshRemoteImpl(FtpClientEx ftpClientEx)
    {
        if (RemoteSelectedItem == null)
            await ftpClientEx.Refresh("/");
        else
            await ftpClientEx.Refresh(RemoteSelectedItem.Path);

        await ftpClientEx.Refresh("@SYS");
        LoadRemoteItems(ftpClientEx);
    }

    private Task RefreshLocalImpl()
    {
        LoadLocalItems(_localRootPath);
        return Task.CompletedTask;
    }

    public static Uri GenerateUri(string baseUri, ushort deviceFullId, DeviceClass @class) =>
        new($"{baseUri}?id={deviceFullId}&class={@class:G}");
    
    #region Load items

    private void LoadLocalItems(string path)
    {
        var items = new ObservableCollection<FileSystemItemViewModel>();
    
        foreach (var directory in Directory.GetDirectories(path))
            items.Add(new FileSystemItemViewModel(directory, true));
        
        foreach (var file in Directory.GetFiles(path))
            items.Add(new FileSystemItemViewModel(file, false));
        
        _localSource.Edit(updater =>
        {
            updater.Clear();
            updater.AddOrUpdate(items);
        });
    }

    private void LoadRemoteItems(FtpClientEx ftpClientEx)
    { 
        ftpClientEx.Entries
            .TransformToTree(e => e.ParentPath)
            .Transform(x => new FileSystemItemViewModel(x))
            .DisposeMany()
            .Bind(out var tree)
            .Subscribe();
        
        _remoteSource.Edit(updater =>
        {
            updater.Clear();
            updater.AddOrUpdate(tree);
        });
    }

    private static void ExpandParents(FileSystemItemViewModel item)
    {
        var parent = item.Parent;
        while (parent != null)
        {
            parent.IsExpanded = true;
            parent = parent.Parent;
        }
    }
    
    private static bool MatchesSearch(FileSystemItemViewModel item, string? search)
    {
        if (search == null) return true;
        return item.Name.Contains(search, StringComparison.OrdinalIgnoreCase) 
               || item.Children.Any(child => MatchesSearch(child, search));
    }

    private static FileSystemItemViewModel? FindFirstMatchingItem(IEnumerable<FileSystemItemViewModel> items, string? search)
    {
        if (search == null) return null;
        
        foreach (var item in items)
        {
            if (item.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
                return item;

            var match = FindFirstMatchingItem(item.Children, search);
            if (match != null)
                return match;
        }
        return null;
    }

    public static string ConvertBytesToReadableSize(long fileSizeInBytes)
    {
        string[] sizes = [
            RS.Unit_Byte_Abbreviation, 
            RS.Unit_Kilobyte_Abbreviation, 
            RS.Unit_Megabyte_Abbreviation,
            RS.Unit_Gigabyte_Abbreviation,
            RS.Unit_Terabyte_Abbreviation
        ];
        double len = fileSizeInBytes;
        var order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    #endregion
}

public class FileSystemItemViewModel : DisposableReactiveObject
{
    private readonly ReadOnlyObservableCollection<FileSystemItemViewModel> _children;
    
    public FileSystemItemViewModel(string entry, bool isDirectory,
        FileSystemItemViewModel? parent = null)
    {
        Parent = parent;
        
        if (isDirectory)
        {
            _children = LoadItems(entry, this);
            IsDirectory = true;
        }
        else
        {
            _children = new ReadOnlyObservableCollection<FileSystemItemViewModel>([]);
            IsFile = true;
            Size = VehicleFileBrowserViewModel.ConvertBytesToReadableSize(new FileInfo(entry).Length);
        }
        
        Name = System.IO.Path.GetFileName(entry);
        EditedName = Name;
        Path = entry;

        Init();
    }
    
    public FileSystemItemViewModel(Node<IFtpEntry, string> node,
        FileSystemItemViewModel? parent = null)
    {
        Parent = parent;
        node.Children
            .Connect()
            .Transform(c => new FileSystemItemViewModel(c, this))
            .Bind(out _children)
            .Subscribe();
        Name = node.Item.Name;
        EditedName = Name;
        Path = node.Item.Path;
        IsDirectory = node.Item.Type is FtpEntryType.Directory;
        IsFile = node.Item.Type is FtpEntryType.File;

        if (IsFile) Size = VehicleFileBrowserViewModel.ConvertBytesToReadableSize(((FtpFile)node.Item).Size);
        
        Init();
    }


    private void Init()
    {
        this.WhenValueChanged(x => x.IsSelected)
            .Subscribe(b =>
            {
                if (b) return;
                IsInEditMode = false;
            });
        this.WhenAnyValue(x => x.IsExpanded)
            .Where(expanded => expanded)
            .Subscribe(_ => OnExpanded());
        EndEditCommand = ReactiveCommand.CreateFromTask(EndEditImpl);
    }
    
    private void OnExpanded()
    {
        IsSelected = true;
    }

    private static ReadOnlyObservableCollection<FileSystemItemViewModel> LoadItems(string path, FileSystemItemViewModel? parent)
    {
        var items = new ObservableCollection<FileSystemItemViewModel>();
    
        foreach (var directory in Directory.GetDirectories(path))
            items.Add(new FileSystemItemViewModel(directory, true, parent));
        
        foreach (var file in Directory.GetFiles(path))
            items.Add(new FileSystemItemViewModel(file, false, parent));

        return new ReadOnlyObservableCollection<FileSystemItemViewModel>(items);
    }
    
    private Task EndEditImpl()
    {
        IsInEditMode = false;
        if (Name != EditedName) IsEditingSuccess = true;
        return Task.CompletedTask;
    }
    
    public ReactiveCommand<Unit, Unit> EndEditCommand { get; set; } = null!;
    public ReadOnlyObservableCollection<FileSystemItemViewModel> Children => _children;
    public FileSystemItemViewModel? Parent { get; }
    public string Name { get; set; }
    public string Path { get; set; }
    public bool IsDirectory { get; set; }
    public bool IsFile { get; set; }
    public string? Size { get; set; }
    public bool IsEditingSuccess { get; set; }
    [Reactive] public bool IsExpanded { get; set; }
    [Reactive] public bool IsSelected { get; set; } 
    [Reactive] public bool IsInEditMode { get; set; }
    [Reactive] public string EditedName { get; set; }
}
