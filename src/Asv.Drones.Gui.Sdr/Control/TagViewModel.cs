using System.Windows.Input;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Asv.Mavlink.V2.AsvSdr;
using Avalonia.Media;
using Material.Icons;
using ReactiveUI;

namespace Asv.Drones.Gui.Sdr;

public static class SdrTagViewModelHelper
{
    public static HierarchicalStoreEntryTagViewModel ConvertToTag(AsvSdrRecordTagPayload arg)
    {
        return new HierarchicalStoreEntryTagViewModel
        {
            Icon = MaterialIconKind.Tag,
            Color = ConvertBrush(arg.TagType),
            Name = $"{MavlinkTypesHelper.GetString(arg.TagName)}:{ConvertValue(arg)}",
        };
    }

    private static string ConvertValue(AsvSdrRecordTagPayload tag)
    {
        return tag.TagType switch
        {
            AsvSdrRecordTagType.AsvSdrRecordTagTypeUint64 => BitConverter.ToUInt64(tag.TagValue).ToString("N"),
            AsvSdrRecordTagType.AsvSdrRecordTagTypeInt64 => BitConverter.ToInt64(tag.TagValue).ToString("N"),
            AsvSdrRecordTagType.AsvSdrRecordTagTypeReal64 => BitConverter.ToDouble(tag.TagValue).ToString("F7"),
            AsvSdrRecordTagType.AsvSdrRecordTagTypeString8 => MavlinkTypesHelper.GetString(tag.TagValue),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static IBrush ConvertBrush(AsvSdrRecordTagType argTagType)
    {
        return argTagType switch
        {
            AsvSdrRecordTagType.AsvSdrRecordTagTypeUint64 => Brushes.BlueViolet,
            AsvSdrRecordTagType.AsvSdrRecordTagTypeInt64 => Brushes.Orange,
            AsvSdrRecordTagType.AsvSdrRecordTagTypeReal64 => Brushes.DarkGreen,
            AsvSdrRecordTagType.AsvSdrRecordTagTypeString8 => Brushes.Pink,
            _ => throw new ArgumentOutOfRangeException(nameof(argTagType), argTagType, null)
        };
    }
}

public class TagViewModel:ReactiveObject
{
    public TagViewModel(TagId id, string name)
    {
        Name = name;
        TagId = id;
    }
    public TagId TagId { get; }
    public string Name { get; }
    public ICommand? Remove { get; set; }
}


public class LongTagViewModel : TagViewModel
{
    public LongTagViewModel(TagId id, string name, long value) : base(id,name)
    {
        Value = value;
    }
    public long Value { get; set; }
}

public class ULongTagViewModel : TagViewModel
{
    public ULongTagViewModel(TagId id, string name, ulong value) : base(id,name)
    {
        Value = value;
    }
    public ulong Value { get; set; }
}

public class DoubleTagViewModel : TagViewModel
{
    public DoubleTagViewModel(TagId id, string name, double value) : base(id,name)
    {
        Value = value;
    }
    public double Value { get; set; }
}

public class StringTagViewModel : TagViewModel
{
    public StringTagViewModel(TagId id, string name, string value) : base(id,name)
    {
        Value = value;
    }
    public string Value { get; set; }
}