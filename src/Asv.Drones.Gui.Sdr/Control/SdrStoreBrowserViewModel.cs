using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Avalonia;
using Avalonia.Controls;
using DocumentFormat.OpenXml;
using DynamicData;
using DynamicData.Binding;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Sdr;

[Export]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class SdrStoreBrowserViewModel : ViewModelBase
{
    private readonly ISdrStoreService _svc;
    public const string UriString = "asv:sdr.store.browser";
    
    private readonly SourceCache<IListDataStoreEntry<Guid>,Guid> _source;
    private readonly ReadOnlyObservableCollection<SdrStoreEntityViewModel> _tree;
    private readonly Subject<Func<IListDataStoreEntry<Guid>,bool>> _filterPipe;
    private readonly ILocalizationService _loc;

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
            .TransformToTree(x => x.ParentId)
            .Transform(x => new SdrStoreEntityViewModel(x,this))
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
                Name = "Record 4", 
                ParentId = Guid.Empty, 
                Type = StoreEntryType.File
            });
        }
    }
    [ImportingConstructor]
    public SdrStoreBrowserViewModel(ISdrStoreService svc, ILocalizationService loc, ILogService log) : this()
    {
        _svc = svc ?? throw new ArgumentNullException(nameof(svc));
        _loc = loc ?? throw new ArgumentNullException(nameof(loc));
        
        Refresh = new CancellableCommandWithProgress<Unit, Unit>(RefreshImpl, "SDR Viewer", log).DisposeItWith(Disposable);
        
        AddFolder = ReactiveCommand.Create(AddFolderImpl)
            .DisposeItWith(Disposable);
        AddFolder.ThrownExceptions
            .Subscribe(ex => log.Error("AFIS",$"Add new folder",ex))
            .DisposeItWith(Disposable);

        Refresh.ExecuteSync();

        Move = ReactiveCommand.CreateFromTask(async () =>
        {
            var viewModel = new MoveDialogViewModel(this);
            var selectedEntry = SelectedItem;
            var dialog = new ContentDialog
            {
                Title = RS.SdrStoreEntityViewModel_MoveDialog_Title,
                PrimaryButtonText = RS.SdrStoreEntityViewModel_MoveDialog_PrimaryButton,
                SecondaryButtonText = RS.SdrStoreEntityViewModel_MoveDialog_SecondaryButton,
                PrimaryButtonCommand = ReactiveCommand.Create(() =>
                {
                    Guid newParentId;
                    if (SelectedItem != null)
                    {
                        newParentId = SelectedItem.Type == StoreEntryType.Folder ? SelectedItem.EntryId : SelectedItem.ParentId;
                    }
                    else
                    {
                        newParentId = _svc.Store.RootFolderId;
                    }
                    _svc.Store.MoveEntry(selectedEntry.EntityId, newParentId);
                    _svc.Store.TryGetEntry(selectedEntry.EntityId, out var entity);
                    
                    Refresh.ExecuteSync();
                })
            };
            dialog.Content = viewModel;
            var result = await dialog.ShowAsync();
        }).DisposeItWith(Disposable);
        
        this.WhenAnyValue(_ => _.SelectedItem)
            .Subscribe(_ =>
            {
                if (_ != null)
                {
                    IsAnySelected = true;
                    if (SelectedItem.IsFolder)
                    {
                        TotalFileSamples = "";
                        TotalFileSize = "";
                        return;
                    }
                    var res = _svc.Store.Open(_.EntryId);
                    TotalFileSamples = string.Format(RS.SdrStoreBrowserViewModel_TotalFileSamples, res.File.Count);
                    TotalFileSize = string.Format(RS.SdrStoreBrowserViewModel_TotalFileSize, _loc.ByteSize.ConvertToStringWithUnits(res.File.ByteSize));    
                    return;
                }
                IsAnySelected = false;
            })
            .DisposeItWith(Disposable);
    }

    public CancellableCommandWithProgress<Unit,Unit> Refresh { get; set; }

    private Task<Unit> RefreshImpl(Unit arg, IProgress<double> progress, CancellationToken cancel)
    {
        return Task.Run(() =>
        {
            _source.Clear();
            _source.AddOrUpdate(_svc.Store.GetEntries());
            
            return Unit.Default;
        }, cancel);
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
        Refresh.Command.Execute().Subscribe(_ =>
        {
            foreach (var item in _tree)
            {
                item.Find(newId);
            }
        });
    }
    
    [Reactive]
    public SdrStoreEntityViewModel? SelectedItem { get; set; }
    
    [Reactive]
    public bool IsAnySelected { get; set; }
    
    public ReadOnlyObservableCollection<SdrStoreEntityViewModel> Items => _tree;

    [Reactive]
    public string TotalFileSamples { get; set; }
    
    [Reactive]
    public string TotalFileSize { get; set; }

    [Reactive]
    public string SearchText { get; set; }

    public ReactiveCommand<Unit,Unit> AddFolder { get; }

    public ReactiveCommand<Unit,Unit> Move { get; }
    
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
        Refresh.ExecuteSync();
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

    public void TrySelect(Guid entityId)
    {
         var item = Items.FirstOrDefault(_ => _.EntryId == entityId);
         if (item == null) return;
         SelectedItem = item;
    }
}