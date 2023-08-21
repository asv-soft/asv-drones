using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Asv.Mavlink.V2.AsvSdr;
using DynamicData;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;
using AsvSdrHelper = Asv.Mavlink.AsvSdrHelper;

namespace Asv.Drones.Gui.Sdr;

public class SdrStoreEntityViewModel : ViewModelBaseWithValidation
{
    private ReadOnlyObservableCollection<SdrStoreEntityViewModel> _items;
    private ReadOnlyObservableCollection<SdrPayloadRecordViewModel>? _tags;

    public SdrStoreEntityViewModel(Node<IHierarchicalStoreEntry<Guid>,Guid> node, SdrStoreBrowserViewModel context):base("asv:sdr-browser.entryr?id="+node.Item.Id)
    {
        ParentId = node.Parent.HasValue ? node.Parent.Value.Item.Id : Guid.Empty;
        EntityId = node.Item.Id;
        Name = node.Item.Name ?? "";
        Type = node.Item.Type;
        EntryId = node.Item.Id;
        
        node.Children.Connect()
            .Transform(_ => new SdrStoreEntityViewModel(_,context))
            .Bind(out _items)
            .DisposeMany()
            .Subscribe()
            .DisposeItWith(Disposable);
        
        IsFolder = Type == FolderStoreEntryType.Folder;
        IsRecord = Type == FolderStoreEntryType.File;
        Delete = ReactiveCommand.CreateFromTask(async () =>
        {
            var dialog = new ContentDialog
            {
                Title = RS.SdrStoreEntityViewModel_DeleteDialog_Title,
                PrimaryButtonText = RS.SdrStoreEntityViewModel_DeleteDialog_PrimaryButton,
                SecondaryButtonText = RS.SdrStoreEntityViewModel_DeleteDialog_SecondaryButton,
                Content = string.Format(RS.SdrStoreEntityViewModel_DeleteDialog_Content, Name),
                PrimaryButtonCommand = ReactiveCommand.Create(() =>
                {
                    context.DeleteEntity(EntryId);
                })
            };
            var result = await dialog.ShowAsync();
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
        Move = context.Move;

        if (IsRecord)
        {
            this.ValidationRule(x => x.Name, _ =>
                {
                    if (_.IsNullOrWhiteSpace()) return false;
                    try
                    {
                        AsvSdrHelper.CheckRecordName(_);
                        return true;
                    }
                    catch(Exception e)
                    {
                    
                    }
                    return false;
                }, _ => RS.SdrStoreEntityViewModel_Name_Validation_ErrorMessage)
                .DisposeItWith(Disposable);
        }
    }

    private Task UpdateImpl(CancellationToken arg)
    {
        return Task.Delay(1);
    }

    [Reactive]
    public bool IsInEditNameMode { get; set; }
    public Guid EntryId { get; set; }

    public ReadOnlyObservableCollection<SdrStoreEntityViewModel> Items
    {
        get => _items;
        set => _items = value;
    }

    [Reactive]
    public string Name { get; set; }
    public FolderStoreEntryType Type { get; }
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
    public ReactiveCommand<Unit,Unit> Move { get; }
    public ReactiveCommand<Unit,Unit> Update { get; }
    public ReadOnlyObservableCollection<SdrPayloadRecordViewModel>? Tags => _tags;
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