using System.Collections.ObjectModel;
using System.Reactive;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Sdr;

public class SdrStoreEntityViewModel:ViewModelBase
{
    private readonly ReadOnlyObservableCollection<SdrStoreEntityViewModel> _items;
    private ReadOnlyObservableCollection<SdrPayloadRecordViewModel> _tags;

    public SdrStoreEntityViewModel(Node<IListDataStoreEntry<Guid>,Guid> node, SdrStoreBrowserViewModel context):base("asv:sdr-browser.entryr?id="+node.Item.Id)
    {
        ParentId = node.Parent.HasValue ? node.Parent.Value.Item.Id : Guid.Empty;
        EntityId = node.Item.Id;
        Name = node.Item.Name;
        Type = node.Item.Type;
        EntryId = node.Item.Id;
        node.Children.Connect()
            .Transform(_ => new SdrStoreEntityViewModel(_,context))
            .Bind(out _items)
            .DisposeMany()
            .Subscribe()
            .DisposeItWith(Disposable);
        IsFolder = Type == StoreEntryType.Folder;
        IsRecord = Type == StoreEntryType.File;
        Delete = ReactiveCommand.Create(() =>
        {
            context.DeleteEntity(EntryId);
        });
        BeginEdit = ReactiveCommand.Create(() =>
        {
            IsInEditNameMode = true;
        });
        EndEdit = ReactiveCommand.Create(() =>
        {
            IsInEditNameMode = false;
            context.RenameEntity(EntryId,Name);
        });
        Update = ReactiveCommand.CreateFromTask(UpdateImpl).DisposeItWith(Disposable);
        
        
    }

    private Task UpdateImpl(CancellationToken arg)
    {
        throw new NotImplementedException();
    }

    [Reactive]
    public bool IsInEditNameMode { get; set; }
    public Guid EntryId { get; }
    public ReadOnlyObservableCollection<SdrStoreEntityViewModel> Items => _items;
    [Reactive]
    public string Name { get; set; }
    public StoreEntryType Type { get; }
    public Guid ParentId { get; set; }
    [Reactive]
    public bool IsExpanded { get; set; }
    [Reactive]
    public bool IsSelected { get; set; }
    
    [Reactive]
    public bool IsFolder { get; set; }
    [Reactive]
    public bool IsRecord  { get; set; }

    public ReactiveCommand<Unit,Unit> Delete { get; }
    public ReactiveCommand<Unit,Unit> BeginEdit { get; }
    public ReactiveCommand<Unit,Unit> EndEdit { get; }
    
    public ReactiveCommand<Unit,Unit> Update { get; }
    public ReadOnlyObservableCollection<SdrPayloadRecordViewModel> Tags => _tags;
    public Guid EntityId { get; }


    public SdrStoreEntityViewModel? Find(Guid id)
    {
        if (EntryId == id)
        {
            return this;
        }

        foreach (var item in Items)
        {
            var found = item.Find(id);
            if (found != null)
            {
                IsExpanded = true;
                return found;
            }
        }

        return null;
    }

}