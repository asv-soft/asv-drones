using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Avalonia.Controls;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Sdr;

public class SdrRecordViewModel:ViewModelBase
{
    private readonly ReadOnlyObservableCollection<TagViewModel> _tagItems;
    private readonly ReadOnlyObservableCollection<TagViewModel> _items;

    #region URI
    public const string UriString = SdrViewModel.UriString + ".rec";
    public static Uri GenerateUri(ushort deviceFullId, IAsvSdrClientRecord rec) => new(UriString + $"?id={deviceFullId}&rec={rec.Id}");
    
    #endregion

    public SdrRecordViewModel():base(new Uri("asv:designTime"))
    {
        if (Design.IsDesignMode)
        {
            Name = "Record1";
            Description = "365 rec. (45 kb)";
            _tagItems = new ReadOnlyObservableCollection<TagViewModel>(new ObservableCollection<TagViewModel>(new List<TagViewModel>
            {
                new() {Name = "Tag1: 65.4654"},
                new() {Name = "Tag2: 4554654"},
                new() {Name = "Tag2: value1"},
            }));
        }
    }
    
    public SdrRecordViewModel(ushort deviceFullId, IAsvSdrClientRecord record,ILogService log):base(GenerateUri(deviceFullId,record))
    {
        record.Name.Subscribe(_=>Name = _).DisposeItWith(Disposable);
        record.Created.Subscribe(_=>CreatedDateTime = _).DisposeItWith(Disposable);
        record.DataType.Subscribe(_=>Description = _.ToString("G")).DisposeItWith(Disposable);
        
        DownloadTags = ReactiveCommand.CreateFromTask(cancel =>
                record.UploadTagList(new Progress<double>(_ => TagsProgress = _), cancel))
            .DisposeItWith(Disposable);
        DownloadTags.ThrownExceptions.Subscribe(_ =>
            {
                if (Name != null) log.Error(Name, "Error to download records", _);
            })
            .DisposeItWith(Disposable);
        record.Tags
            .Transform(_=>new TagViewModel(_))
            .SortBy(_=>_.Name)
            .Bind(out _tagItems)
            .DisposeMany()
            .Subscribe()
            .DisposeItWith(Disposable);
        record.ByteSize.Subscribe(_=>Description = $"{record.DataCount.Value} rec. ({record.ByteSize} bytes)").DisposeItWith(Disposable);
        record.DataCount.Subscribe(_=>Description = $"{record.DataCount.Value} rec. ({record.ByteSize} bytes)").DisposeItWith(Disposable);
        
    }
    
    [Reactive]
    public string? Name { get; set; }
    [Reactive]
    public DateTime CreatedDateTime { get; set; }
    [Reactive]
    public string Description { get; set; }
    [Reactive]
    public double TagsProgress { get; set; }
    public ReactiveCommand<Unit,bool> DownloadTags { get; }
    public ReadOnlyObservableCollection<TagViewModel> TagItems => _tagItems;
    public ReadOnlyObservableCollection<TagViewModel> Items => _items;
}

public class TagViewModel:ReactiveObject
{
    public TagViewModel()
    {
        
    }
    
    public TagViewModel(AsvSdrClientRecordTag tag)
    {
        Name = tag.ToString();
    }
    
    public string Name { get; set; } = null!;
    
    public ICommand Remove { get; set; }
}

public class LongTagViewModel : TagViewModel
{
    public LongTagViewModel()
    {
        
    }
    
    public LongTagViewModel(AsvSdrClientRecordTag tag) : base(tag)
    {
        Value = tag.GetInt64();
    }
    
    public long Value { get; set; }
}

public class ULongTagViewModel : TagViewModel
{
    public ULongTagViewModel()
    {
        
    }
    
    public ULongTagViewModel(AsvSdrClientRecordTag tag) : base(tag)
    {
        Value = tag.GetUint64();
    }
    
    public ulong Value { get; set; }
}

public class DoubleTagViewModel : TagViewModel
{
    public DoubleTagViewModel()
    {
        
    }
    
    public DoubleTagViewModel(AsvSdrClientRecordTag tag) : base(tag)
    {
        Value = tag.GetReal64();
    }
    
    public double Value { get; set; }
}

public class StringTagViewModel : TagViewModel
{
    public StringTagViewModel()
    {
        
    }
    
    public StringTagViewModel(AsvSdrClientRecordTag tag) : base(tag)
    {
        Value = tag.GetString();
    }
    
    public string Value { get; set; }
}