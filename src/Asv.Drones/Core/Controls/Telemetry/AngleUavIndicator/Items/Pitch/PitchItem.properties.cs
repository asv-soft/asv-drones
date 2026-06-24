using Avalonia;

namespace Asv.Drones;

public partial class PitchItem
{
    public static readonly DirectProperty<PitchItem, string> TitleProperty =
        AvaloniaProperty.RegisterDirect<PitchItem, string>(
            nameof(Title),
            _ => _.Title,
            (_, value) => _.Title = value
        );

    public string Title
    {
        get;
        set => SetAndRaise(TitleProperty, ref field, value);
    }

    public static readonly DirectProperty<PitchItem, double> ValueProperty =
        AvaloniaProperty.RegisterDirect<PitchItem, double>(
            nameof(Value),
            _ => _.Value,
            (_, value) => _.Value = value
        );

    public double Value
    {
        get;
        set => SetAndRaise(ValueProperty, ref field, value);
    }

    public static readonly DirectProperty<PitchItem, bool> IsVisibleProperty =
        AvaloniaProperty.RegisterDirect<PitchItem, bool>(
            nameof(IsVisible),
            _ => _.IsVisible,
            (_, value) => _.IsVisible = value
        );

    public bool IsVisible
    {
        get;
        set => SetAndRaise(IsVisibleProperty, ref field, value);
    }

    public static readonly DirectProperty<PitchItem, Point> StartLineProperty =
        AvaloniaProperty.RegisterDirect<PitchItem, Point>(
            nameof(StartLine),
            _ => _.StartLine,
            (_, value) => _.StartLine = value
        );

    public Point StartLine
    {
        get;
        set => SetAndRaise(StartLineProperty, ref field, value);
    }

    public static readonly DirectProperty<PitchItem, Point> StopLineProperty =
        AvaloniaProperty.RegisterDirect<PitchItem, Point>(
            nameof(StopLine),
            _ => _.StopLine,
            (_, value) => _.StopLine = value
        );

    public Point StopLine
    {
        get;
        set => SetAndRaise(StopLineProperty, ref field, value);
    }
}
