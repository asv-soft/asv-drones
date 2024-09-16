using System.Collections.ObjectModel;
using System.Globalization;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Asv.Common;
using Asv.Mavlink;
using Avalonia.Media;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Api;

public class HierarchicalStoreViewModel : ViewModelBase
{
    protected HierarchicalStoreViewModel(Uri id) : base(id)
    {
        CreateNewFile = ReactiveCommand.Create(CreateNewFileImpl)
            .DisposeItWith(Disposable);
        CreateNewFile.ThrownExceptions
            .Subscribe(ex => OnError(HierarchicalStoreAction.CreateFile, ex))
            .DisposeItWith(Disposable);

        CreateNewFolder = ReactiveCommand.Create(CreateNewFolderImpl)
            .DisposeItWith(Disposable);
        CreateNewFolder.ThrownExceptions
            .Subscribe(ex => OnError(HierarchicalStoreAction.CreateFolder, ex))
            .DisposeItWith(Disposable);

        Refresh = ReactiveCommand.Create(() =>
            {
                var selectedId = SelectedItem?.Id;
                var selectedParentId = SelectedItem?.ParentId;
                RefreshImpl();
                if (TrySelect(selectedId) == false)
                {
                    TrySelect(selectedParentId);
                }
            })
            .DisposeItWith(Disposable);
        Refresh.ThrownExceptions
            .Subscribe(ex => OnError(HierarchicalStoreAction.Refresh, ex))
            .DisposeItWith(Disposable);
    }


    public HierarchicalStoreViewModel() : this(WellKnownUri.UndefinedUri)
    {
        DesignTime.ThrowIfNotDesignMode();
        Items = new ReadOnlyObservableCollection<HierarchicalStoreEntryViewModel>(
            new ObservableCollection<HierarchicalStoreEntryViewModel>(new List<HierarchicalStoreEntryViewModel>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Record1",
                    Tags = new ReadOnlyObservableCollection<HierarchicalStoreEntryTagViewModel>(
                        new ObservableCollection<HierarchicalStoreEntryTagViewModel>(
                            new List<HierarchicalStoreEntryTagViewModel>
                            {
                                new()
                                {
                                    Color = Brushes.CornflowerBlue,
                                    Name = "Latitude: 55.1234567",
                                },
                                new()
                                {
                                    Color = Brushes.DarkOrange,
                                    Name = "Short",
                                },
                                new()
                                {
                                    Color = new SolidColorBrush(Color.Parse("#FBC02D")),
                                    Name = "Longitude: 66.1234567",
                                },
                                new()
                                {
                                    Color = new SolidColorBrush(Color.Parse("#FE8256")),
                                    Name = "Longitude: 66.1234567",
                                },
                                new()
                                {
                                    Color = new SolidColorBrush(Color.Parse("#ACC865")),
                                    Name = "ACC865:66.1234567",
                                },
                                new()
                                {
                                    Color = new SolidColorBrush(Color.Parse("#CD91B6")),
                                    Name = "ShortShort",
                                },
                            })),
                    IsFile = true,
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Folder",
                    IsFolder = true,
                    Items = new ReadOnlyObservableCollection<HierarchicalStoreEntryViewModel>(
                        new ObservableCollection<HierarchicalStoreEntryViewModel>(
                            new List<HierarchicalStoreEntryViewModel>
                            {
                                new()
                                {
                                    Id = Guid.NewGuid(),
                                    Name = "Record 2",
                                    IsFile = true,
                                },
                            }))
                },
            }));
        FolderItems = new ReadOnlyObservableCollection<HierarchicalStoreEntryViewModel>(
            new ObservableCollection<HierarchicalStoreEntryViewModel>(new List<HierarchicalStoreEntryViewModel>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Folder",
                    IsFolder = true,
                    Items = new ReadOnlyObservableCollection<HierarchicalStoreEntryViewModel>(
                        new ObservableCollection<HierarchicalStoreEntryViewModel>(
                            new List<HierarchicalStoreEntryViewModel>
                            {
                                new()
                                {
                                    Id = Guid.NewGuid(),
                                    Name = "Folder 2",
                                    IsFolder = true,
                                },
                            }))
                },
            }));
    }

    [Reactive] public string SearchText { get; set; }
    [Reactive] public string DisplayName { get; set; }
    public virtual ReadOnlyObservableCollection<HierarchicalStoreEntryViewModel> Items { get; }
    public virtual ReadOnlyObservableCollection<HierarchicalStoreEntryViewModel> FolderItems { get; }
    [Reactive] public HierarchicalStoreEntryViewModel? SelectedItem { get; set; }
    public ReactiveCommand<Unit, Unit> CreateNewFolder { get; set; }
    public ReactiveCommand<Unit, Unit> CreateNewFile { get; set; }
    public ReactiveCommand<Unit, Unit> Refresh { get; set; }
    public bool IsHeaderVisible { get; set; } = true;

    public bool TrySelect(object? selectedId)
    {
        if (selectedId == null) return false;
        foreach (var item in Items)
        {
            var find = item.FindAndSelect(selectedId);
            if (find == null) continue;
            find.IsSelected = true;
            return true;
        }

        return false;
    }

    protected virtual void RefreshImpl()
    {
    }

    protected virtual void CreateNewFileImpl()
    {
    }

    protected virtual void CreateNewFolderImpl()
    {
    }

    protected virtual void OnError(HierarchicalStoreAction action, Exception? ex)
    {
    }

    [Reactive] public bool IsCreateFolderAvailable { get; set; } = true;
    [Reactive] public bool IsCreateFileAvailable { get; set; } = true;

    [Reactive] public HierarchicalStoreEntryViewModel? SelectedItemMoveTo { get; set; }
}

public abstract class HierarchicalStoreViewModel<TKey, TFile> : HierarchicalStoreViewModel
    where TFile : IDisposable where TKey : notnull
{
    private readonly IHierarchicalStore<TKey, TFile> _store;
    private readonly ILogService _log;
    private readonly SourceCache<IHierarchicalStoreEntry<TKey>, TKey> _source;
    private readonly ReadOnlyObservableCollection<HierarchicalStoreEntryViewModel> _tree;
    private readonly ReadOnlyObservableCollection<HierarchicalStoreEntryViewModel> _treeFolder;


    public HierarchicalStoreViewModel() : base(WellKnownUri.UndefinedUri)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public HierarchicalStoreViewModel(Uri id, IHierarchicalStore<TKey, TFile> store, ILogService log) : base(id)
    {
        _store = store;
        _log = log;
        _source = new SourceCache<IHierarchicalStoreEntry<TKey>, TKey>(x => x.Id)
            .DisposeItWith(Disposable);
        var filterPipe = new Subject<Func<IHierarchicalStoreEntry<TKey>, bool>>()
            .DisposeItWith(Disposable);
        this.WhenValueChanged(x => x.SearchText)
            .Throttle(TimeSpan.FromMilliseconds(500), RxApp.MainThreadScheduler)
            .Subscribe(search => filterPipe.OnNext(item => search.IsNullOrWhiteSpace() || item.Name.Contains(search)))
            .DisposeItWith(Disposable);

        _source
            .Connect()
            .Filter(filterPipe)
            .TransformToTree(x => x.ParentId)
            .Transform(x =>
                (HierarchicalStoreEntryViewModel)new HierarchicalStoreEntryViewModel<TKey, TFile>(x, this, log))
            .DisposeMany()
            .Bind(out _tree)
            .Subscribe()
            .DisposeItWith(Disposable);

        _source
            .Connect()
            .Filter(x => x.Type == FolderStoreEntryType.Folder)
            .TransformToTree(x => x.ParentId)
            .Transform(x =>
                (HierarchicalStoreEntryViewModel)new HierarchicalStoreEntryViewModel<TKey, TFile>(x, this, log))
            .DisposeMany()
            .Bind(out _treeFolder)
            .Subscribe()
            .DisposeItWith(Disposable);

        this.WhenValueChanged(x => x.SelectedItem)
            .Subscribe(x => x?.Refresh()).DisposeItWith(Disposable);
    }

    protected override void OnError(HierarchicalStoreAction action, Exception? ex)
    {
        _log.Error(DisplayName, $"Error to '{action:G}'", ex);
    }

    protected override void CreateNewFolderImpl()
    {
        TKey parentId;
        if (SelectedItem != null)
        {
            parentId =
                (TKey)(SelectedItem.Type == FolderStoreEntryType.Folder ? SelectedItem.Id : SelectedItem.ParentId);
        }
        else
        {
            parentId = _store.RootFolderId;
        }

        var attempt = 0;
        start:
        var name = $"New folder {++attempt}";
        try
        {
            _store.CreateFolder(GenerateNewId(), name, parentId);
        }
        catch (HierarchicalStoreFolderAlreadyExistException)
        {
            goto start;
        }

        Refresh.Execute().Subscribe(_ => { });
    }

    protected abstract TKey GenerateNewId();

    protected override void CreateNewFileImpl()
    {
        TKey parentId;
        if (SelectedItem != null)
        {
            parentId =
                (TKey)(SelectedItem.Type == FolderStoreEntryType.Folder ? SelectedItem.Id : SelectedItem.ParentId);
        }
        else
        {
            parentId = _store.RootFolderId;
        }

        var attempt = 0;
        start:
        var name = $"New file {++attempt}";
        try
        {
            using var file = _store.CreateFile(GenerateNewId(), name, parentId);
        }
        catch (HierarchicalStoreFolderAlreadyExistException)
        {
            goto start;
        }

        Refresh.Execute().Subscribe(_ => { });
    }

    protected override void RefreshImpl()
    {
        _source.Clear();
        _source.AddOrUpdate(_store.GetEntries());
    }


    public override ReadOnlyObservableCollection<HierarchicalStoreEntryViewModel> Items => _tree;
    public override ReadOnlyObservableCollection<HierarchicalStoreEntryViewModel> FolderItems => _treeFolder;

    public void DeleteEntryImpl(TKey itemId)
    {
        _store.DeleteEntry(itemId);
        Refresh.Execute().Subscribe();
    }

    public void RenameEntryImpl(TKey itemId, string name)
    {
        _store.RenameEntry(itemId, name);
        Refresh.Execute().Subscribe();
    }

    public void MoveEntryImpl(TKey id, TKey parentId)
    {
        _store.MoveEntry(id, parentId);
        Refresh.Execute().Subscribe();
    }

    public IReadOnlyCollection<HierarchicalStoreEntryTagViewModel> GetEntryTags(TKey id)
    {
        var item = _source.Lookup(id);
        return !item.HasValue
            ? ArraySegment<HierarchicalStoreEntryTagViewModel>.Empty
            : InternalGetEntryTags(item.Value);
    }

    protected virtual IReadOnlyCollection<HierarchicalStoreEntryTagViewModel> InternalGetEntryTags(
        IHierarchicalStoreEntry<TKey> itemValue)
    {
        return ArraySegment<HierarchicalStoreEntryTagViewModel>.Empty;
    }


    public virtual string GetEntryDescription(IHierarchicalStoreEntry<TKey> nodeItem)
    {
        var entry = nodeItem as FileSystemHierarchicalStoreEntry<TKey>;

        if (entry == null) return string.Empty;

        if (nodeItem.Type == FolderStoreEntryType.Folder)
        {
            var info = new DirectoryInfo(entry.FullPath);
            return info.CreationTime.ToString(CultureInfo.CurrentCulture);
        }

        if (nodeItem.Type == FolderStoreEntryType.File)
        {
            var info = new FileInfo(entry.FullPath);
            return info.CreationTime.ToString(CultureInfo.CurrentCulture);
        }

        return string.Empty;
    }
}

public enum HierarchicalStoreAction
{
    Refresh,
    CreateFile,
    CreateFolder
}