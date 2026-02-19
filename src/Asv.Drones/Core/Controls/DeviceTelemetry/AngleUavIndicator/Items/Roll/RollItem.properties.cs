using Avalonia;

namespace Asv.Drones;

public partial class RollItem
{
    public static readonly DirectProperty<OldAttitudeIndicator.RollItem, string> TitleProperty =
        AvaloniaProperty.RegisterDirect<OldAttitudeIndicator.RollItem, string>(
            nameof(Title),
            _ => _.Title,
            (_, value) => _.Title = value
        );

    public string Title
    {
        get;
        set => SetAndRaise(TitleProperty, ref field, value);
    }

    public static readonly DirectProperty<OldAttitudeIndicator.RollItem, double> ValueProperty =
        AvaloniaProperty.RegisterDirect<OldAttitudeIndicator.RollItem, double>(
            nameof(Value),
            _ => _.Value,
            (_, value) => _.Value = value
        );

    public double Value
    {
        get;
        set => SetAndRaise(ValueProperty, ref field, value);
    }
}
