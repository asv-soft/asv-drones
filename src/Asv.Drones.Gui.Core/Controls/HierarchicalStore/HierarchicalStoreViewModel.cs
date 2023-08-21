using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Asv.Common;
using Asv.Mavlink;
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
    public HierarchicalStoreEntryViewModel SelectedItem { get; set; }
    public ReactiveCommand<Unit,Unit> CreateNewFolder { get; }
    public ReactiveCommand<Unit,Unit> CreateNewFile { get; }
    public ReactiveCommand<Unit,Unit> Refresh { get; }
    public MaterialIconKind FileIcon { get; } = MaterialIconKind.FileMarkerOutline;
    public MaterialIconKind SelectedFileIcon { get; } = MaterialIconKind.FileMarker;
}

public class HierarchicalStoreEntryViewModel:DisposableReactiveObject
{
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
    [Reactive]
    public bool IsInEditNameMode { get; set; }

    public ReactiveCommand<Unit,Unit> DeleteEntry { get; }
    public ReactiveCommand<Unit,Unit> BeginEditName { get; }
    public ReactiveCommand<Unit,Unit> EndEditName { get; }
}

public class HierarchicalStoreViewModel<TKey,TFile>:HierarchicalStoreViewModel 
    where TFile : IDisposable where TKey : notnull
{
    private readonly IHierarchicalStore<TKey, TFile> _store;
    private readonly SourceCache<IHierarchicalStoreEntry<TKey>,TKey> _source;
    private readonly ReadOnlyObservableCollection<HierarchicalStoreEntryViewModel> _tree;


    public HierarchicalStoreViewModel(Uri id,IHierarchicalStore<TKey,TFile> store):base(id)
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
            .Transform(x=>(HierarchicalStoreEntryViewModel)new HierarchicalStoreEntryViewModel<TKey,TFile>(x,this))
            .Bind(out _tree)
            .Subscribe()
            .DisposeItWith(Disposable);
    }

    public override ReadOnlyObservableCollection<HierarchicalStoreEntryViewModel> Items => _tree;
}

public class HierarchicalStoreEntryViewModel<TKey,TFile>:HierarchicalStoreEntryViewModel 
    where TKey : notnull 
    where TFile : IDisposable
{
    private readonly ReadOnlyObservableCollection<HierarchicalStoreEntryViewModel> _items;
    public HierarchicalStoreEntryViewModel(Node<IHierarchicalStoreEntry<TKey>, TKey> node, HierarchicalStoreViewModel<TKey,TFile> context)
    {
        node.Children.Connect()
            .Transform(x => (HierarchicalStoreEntryViewModel)new HierarchicalStoreEntryViewModel<TKey,TFile>(x,context))
            .Bind(out _items)
            .DisposeMany()
            .Subscribe()
            .DisposeItWith(Disposable);
        IsFolder = node.Item.Type == FolderStoreEntryType.Folder;
        IsFile = node.Item.Type == FolderStoreEntryType.File;
        Name = node.Item.Name ?? "";
        Type = node.Item.Type;
        ParentId = node.Item.ParentId;
    }

    public TKey ParentId { get; set; }

    public override ReadOnlyObservableCollection<HierarchicalStoreEntryViewModel> Items => _items;
}