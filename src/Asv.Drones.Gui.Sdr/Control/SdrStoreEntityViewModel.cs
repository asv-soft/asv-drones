using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Asv.Mavlink.V2.AsvSdr;
using DynamicData;
using FluentAvalonia.UI.Controls;
using Newtonsoft.Json.Converters;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;
using AsvSdrHelper = Asv.Mavlink.AsvSdrHelper;

namespace Asv.Drones.Gui.Sdr;

public class SdrStoreEntityViewModel : ViewModelBaseWithValidation
{
    private readonly SdrStoreBrowserViewModel _context;
    private readonly ReadOnlyObservableCollection<SdrStoreEntityViewModel> _items;
    private readonly ReadOnlyObservableCollection<TagViewModel>? _tags;
    private readonly SourceCache<TagViewModel,TagId> _tagsSource;

    public SdrStoreEntityViewModel(Node<IHierarchicalStoreEntry<Guid>,Guid> node, SdrStoreBrowserViewModel context):base("asv:sdr-browser.entryr?id="+node.Item.Id)
    {
        _context = context;
        ParentId = node.Parent.HasValue ? node.Parent.Value.Item.Id : Guid.Empty;
        Name = node.Item.Name ?? "";
        Type = node.Item.Type;
        EntryId = node.Item.Id;
        
        node.Children.Connect()
            .Transform(_ => new SdrStoreEntityViewModel(_,context))
            .Bind(out _items)
            .DisposeMany()
            .Subscribe()
            .DisposeItWith(Disposable);
        _tagsSource = new SourceCache<TagViewModel, TagId>(x=>x.TagId).DisposeItWith(Disposable);
        _tagsSource.Connect()
            .Bind(out _tags)
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
                PrimaryButtonCommand = ReactiveCommand.Create(() => context.DeleteEntity(EntryId))
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
        Update = ReactiveCommand.Create(UpdateImpl).DisposeItWith(Disposable);
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

    private void UpdateImpl()
    {
        if (!IsRecord) return;
        _tagsSource.Clear();
        using var res = _context.Service.Store.Open(EntryId);
        TotalFileSamples = string.Format(RS.SdrStoreBrowserViewModel_TotalFileSamples, res.File.Count);
        TotalFileSize = string.Format(RS.SdrStoreBrowserViewModel_TotalFileSize,
            _context.Localization.ByteSize.ConvertToStringWithUnits(res.File.ByteSize));
        var metadata = res.File.ReadMetadata();
        foreach (var tag in metadata.Tags)
        {
            _tagsSource.AddOrUpdate(TransformRecordTag(tag));
        }
    }

    [Reactive]
    public bool IsInEditNameMode { get; set; }
    public Guid EntryId { get; }
    
    public ReadOnlyObservableCollection<SdrStoreEntityViewModel> Items => _items;

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

    #region Record info

    [Reactive]
    public string TotalFileSamples { get; set; }
    [Reactive]
    public string TotalFileSize { get; set; }
    public ReadOnlyObservableCollection<TagViewModel>? Tags => _tags;
    
    private TagViewModel TransformRecordTag(AsvSdrRecordTagPayload tag)
    {
        var tagId = MavlinkTypesHelper.GetGuid(tag.TagGuid);
        var recId = MavlinkTypesHelper.GetGuid(tag.RecordGuid);
        var fullTagId = new TagId(tagId,recId); 
        var name = MavlinkTypesHelper.GetString(tag.TagName);
        if (tag.TagType == AsvSdrRecordTagType.AsvSdrRecordTagTypeInt64)
        {
            return new LongTagViewModel(fullTagId, name,  BitConverter.ToInt64(tag.TagValue));
        }
        else if (tag.TagType == AsvSdrRecordTagType.AsvSdrRecordTagTypeUint64)
        {
            return new ULongTagViewModel(fullTagId, name,  BitConverter.ToUInt64(tag.TagValue));
        }
        else if (tag.TagType == AsvSdrRecordTagType.AsvSdrRecordTagTypeReal64)
        {
            return new DoubleTagViewModel(fullTagId, name,  BitConverter.ToDouble(tag.TagValue));
        }
        else if (tag.TagType == AsvSdrRecordTagType.AsvSdrRecordTagTypeString8)
        {
            return new StringTagViewModel(fullTagId, name,  Encoding.ASCII.GetString(tag.TagValue));
        }

        return new TagViewModel(fullTagId, name);
    }
    
    #endregion
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