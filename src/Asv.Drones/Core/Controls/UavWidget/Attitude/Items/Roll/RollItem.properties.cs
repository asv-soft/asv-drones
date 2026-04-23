using Avalonia;

namespace Asv.Drones;

public partial class RollItem
{
    public static readonly DirectProperty<RollItem, string> TitleProperty =
        AvaloniaProperty.RegisterDirect<RollItem, string>(
            nameof(Title),
            _ => _.Title,
            (_, value) => _.Title = value
        );

    private string _title;
    private double _value;

    public string Title
    {
        get => _title;
        set => SetAndRaise(TitleProperty, ref _title, value);
    }

    public static readonly DirectProperty<RollItem, double> ValueProperty =
        AvaloniaProperty.RegisterDirect<RollItem, double>(
            nameof(Value),
            _ => _.Value,
            (_, value) => _.Value = value
        );

    public double Value
    {
        get => _value;
        set => SetAndRaise(ValueProperty, ref _value, value);
    }
}
