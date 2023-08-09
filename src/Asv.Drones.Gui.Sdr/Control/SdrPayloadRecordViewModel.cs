using System.Collections.ObjectModel;
using System.Reactive;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Asv.Mavlink.V2.AsvSdr;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Sdr;

public class SdrPayloadRecordViewModel:ViewModelBase
{
    private ReadOnlyObservableCollection<TagViewModel> _tags;

    public SdrPayloadRecordViewModel(Guid id):base($"asv:sdr.device.browser?id{id}")
    {
        _tags = new ReadOnlyObservableCollection<TagViewModel>(new ObservableCollection<TagViewModel>(new List<TagViewModel>
        {
            new() {Name = "Tag1: 65.4654"},
            new() {Name = "Tag2: 4554654"},
            new() {Name = "Tag2: value1"},
        }));
    }
    
    public SdrPayloadRecordViewModel(ushort deviceId, IAsvSdrClientRecord record, ILogService log, ILocalizationService loc)
        :this(record.Id)
    {
        Record = record;
        
        record.Name.Subscribe(_=>Name = _).DisposeItWith(Disposable);
        record.Created.Subscribe(_=>CreatedDateTime = _).DisposeItWith(Disposable);
        record.DataType.Subscribe(_=>Description = _.ToString("G")).DisposeItWith(Disposable);
        
        record.Tags
            .Transform(TransformRecordTag)
            .SortBy(_=>_.Name)
            .Bind(out _tags)
            .DisposeMany()
            .Subscribe()
            .DisposeItWith(Disposable);
        
        DownloadTags = ReactiveCommand.CreateFromTask(cancel =>
                record.DownloadTagList(new Progress<double>(_ => DownloadTagsProgress = _), cancel))
            .DisposeItWith(Disposable);
        DownloadTags.ThrownExceptions.Subscribe(_ =>
            {
                if (Name != null) log.Error(Name, "Error to download tags", _);
            }).DisposeItWith(Disposable);
    }

    public string Description { get; set; }
    public DateTime CreatedDateTime { get; set; }
    public string Name { get; set; }
    [Reactive]
    public bool IsSelected { get; set; }
    public ReadOnlyObservableCollection<TagViewModel> Tags => _tags;
    public ReactiveCommand<Unit,Unit> Delete { get; }
    
    public double DownloadTagsProgress { get; set; }
    public ReactiveCommand<Unit,bool> DownloadTags { get; }
    public IAsvSdrClientRecord Record { get; }

    private TagViewModel TransformRecordTag(AsvSdrClientRecordTag tag)
    {
        if (tag.Type == AsvSdrRecordTagType.AsvSdrRecordTagTypeInt64)
        {
            return new LongTagViewModel(tag);
        }
        else if (tag.Type == AsvSdrRecordTagType.AsvSdrRecordTagTypeUint64)
        {
            return new ULongTagViewModel(tag);
        }
        else if (tag.Type == AsvSdrRecordTagType.AsvSdrRecordTagTypeReal64)
        {
            return new DoubleTagViewModel(tag);  
        }
        else if (tag.Type == AsvSdrRecordTagType.AsvSdrRecordTagTypeString8)
        {
            return new StringTagViewModel(tag);
        }

        return new TagViewModel(tag);
    }
}