using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
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
    
    private readonly SourceCache<IListDataStoreEntry<Guid>,Guid> _source;
    private readonly ReadOnlyObservableCollection<SdrStoreEntityViewModel> _tree;
    private readonly Subject<Func<IListDataStoreEntry<Guid>,bool>> _filterPipe;
    public SdrStoreBrowserViewModel():base(UriString)
    {
        _source = new SourceCache<IListDataStoreEntry<Guid>,Guid>(_=>_.Id)
            .DisposeItWith(Disposable);
        _filterPipe = new Subject<Func<IListDataStoreEntry<Guid>, bool>>()
            .DisposeItWith(Disposable);
        this.WhenAnyValue(x => x.SearchText)
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
            .TransformToTree(x=>x.ParentId)
            .Transform(x=>new SdrStoreEntityViewModel(x,this))
            .Bind(out _tree)
            .Subscribe()
            .DisposeItWith(Disposable);
        
        if (Design.IsDesignMode)
        {
            _source.AddOrUpdate( new ListDataStoreEntry<Guid>
            {
                Id = new("D09AFD63-4FAC-4C28-AA90-9F574ACE899B"), 
                Name = "Record 1", 
                ParentId = Guid.Empty, 
                Type = StoreEntryType.File
            });
            _source.AddOrUpdate(new ListDataStoreEntry<Guid>
            {
                Id = new("34992EE3-FF6A-4643-A65C-BCEFD5D5A90B"), 
                Name = "Folder 2", 
                ParentId = Guid.Empty,
                Type = StoreEntryType.Folder
            });
            _source.AddOrUpdate(new ListDataStoreEntry<Guid>
            {
                Id = new("011F9C71-3988-4A8B-9EF8-CA2B733E190D"), 
                Name = "Folder 2.1",
                ParentId = new("34992EE3-FF6A-4643-A65C-BCEFD5D5A90B"), 
                Type = StoreEntryType.Folder
            });
            _source.AddOrUpdate( new ListDataStoreEntry<Guid>
            {
                Id = new("43A307ED-94CA-4146-80F2-4AC8B569EC0D"), 
                Name = "Record 2.1.1", 
                ParentId = new("011F9C71-3988-4A8B-9EF8-CA2B733E190D"), 
                Type = StoreEntryType.File
            });
            _source.AddOrUpdate( new ListDataStoreEntry<Guid>
            {
                Id = new("A2DCABA7-68A8-4EC1-87E6-51A97DE176F8"), 
                Name = "Record 3", 
                ParentId = Guid.Empty, 
                Type = StoreEntryType.File
            });
            _source.AddOrUpdate( new ListDataStoreEntry<Guid>
            {
                Id = new("E85A9814-AD8C-4BA2-9DD4-2091231B065E"), 
                Name = "Record 3", 
                ParentId = Guid.Empty, 
                Type = StoreEntryType.File
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
        Guid parentId;
        if (SelectedItem != null)
        {
            parentId = SelectedItem.Type == StoreEntryType.Folder ? SelectedItem.EntryId : SelectedItem.ParentId;
        }
        else
        {
            parentId = _svc.Store.RootFolderId;
        }
        
        var attempt = 0;
        start:
        var name = $"New folder ({++attempt})";
        try
        {
            newId = _svc.Store.CreateFolder(parentId,name);
        }
        catch (ListDataFolderAlreadyExistException)
        {
            goto start;
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
        _source.AddOrUpdate(_svc.Store.GetEntries());
    }
    public void DeleteEntity(Guid id)
    {
        if (_svc.Store.TryGetEntry(id, out var ent) == false) return;
        switch (ent.Type)
        {
            case StoreEntryType.File:
                _svc.Store.DeleteFile(id);
                break;
            case StoreEntryType.Folder:
                _svc.Store.DeleteFolder(id);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        RefreshImpl();
    }

    public void RenameEntity(Guid id, string name)
    {
        if (_svc.Store.TryGetEntry(id, out var ent) == false) return;
        switch (ent.Type)
        {
            case StoreEntryType.File:
                _svc.Store.RenameFile(id,name);
                break;
            case StoreEntryType.Folder:
                _svc.Store.RenameFolder(id,name);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}