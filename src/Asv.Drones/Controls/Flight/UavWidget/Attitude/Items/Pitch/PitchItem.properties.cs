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
        get => _title;
        set => SetAndRaise(TitleProperty, ref _title, value);
    }

    public static readonly DirectProperty<PitchItem, double> ValueProperty =
        AvaloniaProperty.RegisterDirect<PitchItem, double>(
            nameof(Value),
            _ => _.Value,
            (_, value) => _.Value = value
        );

    public double Value
    {
        get => _value;
        set => SetAndRaise(ValueProperty, ref _value, value);
    }

    public static readonly DirectProperty<PitchItem, bool> IsVisibleProperty =
        AvaloniaProperty.RegisterDirect<PitchItem, bool>(
            nameof(IsVisible),
            _ => _.IsVisible,
            (_, value) => _.IsVisible = value
        );

    public bool IsVisible
    {
        get => _isVisible;
        set => SetAndRaise(IsVisibleProperty, ref _isVisible, value);
    }

    public static readonly DirectProperty<PitchItem, Point> StartLineProperty =
        AvaloniaProperty.RegisterDirect<PitchItem, Point>(
            nameof(StartLine),
            _ => _.StartLine,
            (_, value) => _.StartLine = value
        );

    public Point StartLine
    {
        get => _startLine;
        set => SetAndRaise(StartLineProperty, ref _startLine, value);
    }

    public static readonly DirectProperty<PitchItem, Point> StopLineProperty =
        AvaloniaProperty.RegisterDirect<PitchItem, Point>(
            nameof(StopLine),
            _ => _.StopLine,
            (_, value) => _.StopLine = value
        );

    public Point StopLine
    {
        get => _stopLine;
        set => SetAndRaise(StopLineProperty, ref _stopLine, value);
    }
}
