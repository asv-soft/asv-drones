using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Asv.Mavlink;
using Asv.Common;
using Avalonia.Controls;
using DynamicData;
using DynamicData.Binding;
using Material.Icons;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Core;


public class HierarchicalStoreViewModel : ViewModelBase
{

    protected HierarchicalStoreViewModel(Uri id) : base(id)
    {
        
    }
    public HierarchicalStoreViewModel():base($"asv:{Guid.NewGuid()}")
    {
        if (Design.IsDesignMode)
        {
            Items = new ReadOnlyObservableCollection<HierarchicalStoreEntryViewModel>(
                new ObservableCollection<HierarchicalStoreEntryViewModel>(new List<HierarchicalStoreEntryViewModel>
                {
                    new HierarchicalStoreEntryViewModel
                    {
                        Name = "Record1"
                    },
                    
                }));
        }
    }
    
    [Reactive]
    public string SearchText { get; set; }
    
    public virtual ReadOnlyObservableCollection<HierarchicalStoreEntryViewModel> Items { get; }
    [Reactive]
    public HierarchicalStoreEntryViewModel? SelectedItem { get; set; }
    public ReactiveCommand<Unit,Unit> CreateNewFolder { get; set; }
    public ReactiveCommand<Unit,Unit> CreateNewFile { get; set; }
    public ReactiveCommand<Unit,Unit> Refresh { get; set; }
    public MaterialIconKind FileIcon { get; } = MaterialIconKind.FileMarkerOutline;
    public MaterialIconKind SelectedFileIcon { get; } = MaterialIconKind.FileMarker;
}

public class HierarchicalStoreEntryViewModel:DisposableReactiveObject
{
    public object Id { get; set; }
    public object ParentId { get; set; }
    public bool IsFolder { get; set; }
    public bool IsFile { get; set; }
    public FolderStoreEntryType Type { get; set; }

    [Reactive]
    public string Name { get; set; }
    [Reactive]
    public bool IsExpanded { get; set; }
    [Reactive]
    public bool IsSelected { get; set; }
    
    public virtual ReadOnlyObservableCollection<HierarchicalStoreEntryViewModel> Items { get; }
    [Reactive] public bool IsInEditNameMode { get; set; } = false;

    public ReactiveCommand<Unit,Unit> DeleteEntry { get; set; }
    public ReactiveCommand<Unit,Unit> BeginEditName { get; set; }
    public ReactiveCommand<Unit,Unit> EndEditName { get; set; }
}

public abstract class HierarchicalStoreViewModel<TKey,TFile>:HierarchicalStoreViewModel 
    where TFile : IDisposable where TKey : notnull
{
    private readonly IHierarchicalStore<TKey, TFile> _store;
    private readonly SourceCache<IHierarchicalStoreEntry<TKey>,TKey> _source;
    private readonly ReadOnlyObservableCollection<HierarchicalStoreEntryViewModel> _tree;


    public HierarchicalStoreViewModel():base(new Uri($"asv:{Guid.NewGuid()}"))
    {
        
    }
    public HierarchicalStoreViewModel(Uri id,IHierarchicalStore<TKey,TFile> store, ILogService log):base(id)
    {
        _store = store;
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
            .TransformToTree(x=>x.ParentId)
            .Transform(x=>(HierarchicalStoreEntryViewModel)new HierarchicalStoreEntryViewModel<TKey,TFile>(x,this,log))
            .Bind(out _tree)
            .Subscribe()
            .DisposeItWith(Disposable);
        
        Refresh = ReactiveCommand.Create(RefreshImpl)
            .DisposeItWith(Disposable);
        Refresh.ThrownExceptions
            .Subscribe(ex => log.Error("Store",$"Refresh store",ex))
            .DisposeItWith(Disposable);
        
        CreateNewFile = ReactiveCommand.Create(CreateNewFileImpl)
            .DisposeItWith(Disposable);
        CreateNewFile.ThrownExceptions
            .Subscribe(ex => log.Error("Store",$"Add new file",ex))
            .DisposeItWith(Disposable);
        
        CreateNewFolder = ReactiveCommand.Create(CreateNewFolderImpl)
            .DisposeItWith(Disposable);
        CreateNewFolder.ThrownExceptions
            .Subscribe(ex => log.Error("Store",$"Add new folder",ex))
            .DisposeItWith(Disposable);
    }

    private void CreateNewFolderImpl()
    {
        TKey parentId;
        if (SelectedItem != null)
        {
            parentId = (TKey)(SelectedItem.Type == FolderStoreEntryType.Folder ? SelectedItem.Id : SelectedItem.ParentId);
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

    private void CreateNewFileImpl()
    {
        TKey parentId;
        if (SelectedItem != null)
        {
            parentId = (TKey)(SelectedItem.Type == FolderStoreEntryType.Folder ? SelectedItem.Id : SelectedItem.ParentId);
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
            using var file = _store.Create(GenerateNewId(), name, parentId);
        }
        catch (HierarchicalStoreFolderAlreadyExistException)
        {
            goto start;
        }
        
        Refresh.Execute().Subscribe(_ => { });
    }

    private void RefreshImpl()
    {
        _source.Clear();
        _source.AddOrUpdate(_store.GetEntries());
    }

    public override ReadOnlyObservableCollection<HierarchicalStoreEntryViewModel> Items => _tree;

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
}

public class HierarchicalStoreEntryViewModel<TKey,TFile>:HierarchicalStoreEntryViewModel 
    where TKey : notnull 
    where TFile : IDisposable
{
    private readonly ReadOnlyObservableCollection<HierarchicalStoreEntryViewModel> _items;
    public HierarchicalStoreEntryViewModel(Node<IHierarchicalStoreEntry<TKey>, TKey> node, HierarchicalStoreViewModel<TKey,TFile> context, ILogService log)
    {
        node.Children.Connect()
            .Transform(x => (HierarchicalStoreEntryViewModel)new HierarchicalStoreEntryViewModel<TKey,TFile>(x,context,log))
            .Bind(out _items)
            .DisposeMany()
            .Subscribe()
            .DisposeItWith(Disposable);
        IsFolder = node.Item.Type == FolderStoreEntryType.Folder;
        IsFile = node.Item.Type == FolderStoreEntryType.File;
        Name = node.Item.Name ?? "";
        Type = node.Item.Type;
        Id = node.Item.Id;
        ParentId = node.Item.ParentId;
        DeleteEntry = ReactiveCommand.Create(()=>context.DeleteEntryImpl(node.Item.Id))
            .DisposeItWith(Disposable);
        DeleteEntry.ThrownExceptions
            .Subscribe(ex => log.Error("Store",$"Delete",ex))
            .DisposeItWith(Disposable);
        
        BeginEditName = ReactiveCommand.Create(() =>
        {
            IsInEditNameMode = true;
        }).DisposeItWith(Disposable);;
        EndEditName = ReactiveCommand.Create(() =>
        {
            IsInEditNameMode = false;
            context.RenameEntryImpl(node.Item.Id,Name);
        }).DisposeItWith(Disposable);;
        EndEditName.ThrownExceptions
            .Subscribe(ex => log.Error("Store",$"Rename",ex))
            .DisposeItWith(Disposable);
        
    }
    public override ReadOnlyObservableCollection<HierarchicalStoreEntryViewModel> Items => _items;
}