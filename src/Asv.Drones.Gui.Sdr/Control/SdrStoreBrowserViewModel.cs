using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Avalonia.Controls;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Sdr;

[Export]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class SdrStoreBrowserViewModel:ViewModelBase
{
    private readonly ISdrStoreService _svc;
    public const string UriString = "asv:sdr.store.browser";
    
    private readonly SourceCache<ISdrStoreEntry,Guid> _source;
    private readonly ReadOnlyObservableCollection<SdrStoreEntityViewModel> _tree;
    private readonly Subject<Func<ISdrStoreEntry,bool>> _filterPipe;
    public SdrStoreBrowserViewModel():base(UriString)
    {
        _source = new SourceCache<ISdrStoreEntry,Guid>(_=>_.Id)
            .DisposeItWith(Disposable);
        _filterPipe = new Subject<Func<ISdrStoreEntry, bool>>()
            .DisposeItWith(Disposable);
        this.WhenAnyValue(_ => _.SearchText)
            .Throttle(TimeSpan.FromMilliseconds(500), RxApp.MainThreadScheduler)
            .Subscribe(search => _filterPipe.OnNext(item =>
            {
                if (search.IsEmpty()) return true;
                return item.Name.Contains(search);
            }))
            .DisposeItWith(Disposable);
        
        _source
            .Connect()
            .Filter(_filterPipe)
            .TransformToTree(_=>_.ParentId)
            .Transform(_=>new SdrStoreEntityViewModel(_,this))
            .Bind(out _tree)
            .Subscribe()
            .DisposeItWith(Disposable);
        
        if (Design.IsDesignMode)
        {
            _source.AddOrUpdate( new SdrStoreEntry
            {
                Id = new("D09AFD63-4FAC-4C28-AA90-9F574ACE899B"), 
                Name = "Record 1", 
                ParentId = Guid.Empty, 
                Type = StoreEntryType.Record
            });
            _source.AddOrUpdate(new SdrStoreEntry
            {
                Id = new("34992EE3-FF6A-4643-A65C-BCEFD5D5A90B"), 
                Name = "Folder 2", 
                ParentId = Guid.Empty,
                Type = StoreEntryType.Folder
            });
            _source.AddOrUpdate(new SdrStoreEntry
            {
                Id = new("011F9C71-3988-4A8B-9EF8-CA2B733E190D"), 
                Name = "Folder 2.1",
                ParentId = new("34992EE3-FF6A-4643-A65C-BCEFD5D5A90B"), 
                Type = StoreEntryType.Folder
            });
            _source.AddOrUpdate( new SdrStoreEntry
            {
                Id = new("43A307ED-94CA-4146-80F2-4AC8B569EC0D"), 
                Name = "Record 2.1.1", 
                ParentId = new("011F9C71-3988-4A8B-9EF8-CA2B733E190D"), 
                Type = StoreEntryType.Record
            });
            _source.AddOrUpdate( new SdrStoreEntry
            {
                Id = new("A2DCABA7-68A8-4EC1-87E6-51A97DE176F8"), 
                Name = "Record 3", 
                ParentId = Guid.Empty, 
                Type = StoreEntryType.Record
            });
            _source.AddOrUpdate( new SdrStoreEntry
            {
                Id = new("E85A9814-AD8C-4BA2-9DD4-2091231B065E"), 
                Name = "Record 3", 
                ParentId = Guid.Empty, 
                Type = StoreEntryType.Record
            });
        }
    }
    [ImportingConstructor]
    public SdrStoreBrowserViewModel(ISdrStoreService svc, ILogService log) : this()
    {
        _svc = svc ?? throw new ArgumentNullException(nameof(svc));
        
        Refresh = ReactiveCommand.Create(RefreshImpl)
            .DisposeItWith(Disposable);
        Refresh.ThrownExceptions
            .Subscribe(ex => log.Error("SDR Viewer",$"Refresh store entry",ex))
            .DisposeItWith(Disposable);
        
        AddFolder = ReactiveCommand.Create(AddFolderImpl)
            .DisposeItWith(Disposable);
        AddFolder.ThrownExceptions
            .Subscribe(ex => log.Error("AFIS",$"Add new folder",ex))
            .DisposeItWith(Disposable);
        
        
        using var a = Refresh.Execute().Subscribe();
    }

    private void AddFolderImpl()
    {
        Guid newId;
        if (SelectedItem != null)
        {
            var parentId = SelectedItem.Type == StoreEntryType.Folder ? SelectedItem.EntryId : SelectedItem.ParentId;
            newId = _svc.AddNewFolder("New folder", parentId);    
        }
        else
        {
            newId =_svc.AddNewFolder("New folder", Guid.Empty);
        }
        Refresh.Execute().Subscribe(_ =>
        {
            foreach (var item in _tree)
            {
                item.Find(newId);
            }
        });
    }
    
    [Reactive]
    public SdrStoreEntityViewModel? SelectedItem { get; set; }
    
    public ReadOnlyObservableCollection<SdrStoreEntityViewModel> Items => _tree;


    public ReactiveCommand<Unit,Unit> Refresh { get; set; }

    [Reactive]
    public string SearchText { get; set; }

    public ReactiveCommand<Unit,Unit> AddFolder { get; }

    private void RefreshImpl()
    {
        _source.Clear();
        _source.AddOrUpdate(_svc.QueryAll());
    }
    public void DeleteEntity(Guid id)   
    {
        _svc.DeleteEntity(id);
        RefreshImpl();
    }

    public void RenameEntity(Guid id, string name)
    {
        _svc.RenameEntity(id,name);
    }
}