using System.Windows.Input;
using Asv.Mavlink;
using ReactiveUI;

namespace Asv.Drones.Gui.Sdr;

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