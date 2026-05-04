using Avalonia;

namespace Asv.Drones.OldAttitudeIndicator;

public partial class ScaleItem
{
    private readonly double _valueRange;
    private readonly bool _showNegative;
    private readonly double _startPosition;
    private readonly double _positionStep;
    private readonly double _valueOffset;
    private readonly bool _isFixedTitle;

    public static readonly DirectProperty<OldAttitudeIndicator.ScaleItem, bool> IsVisibleProperty =
        AvaloniaProperty.RegisterDirect<OldAttitudeIndicator.ScaleItem, bool>(
            nameof(IsVisible),
            _ => _.IsVisible,
            (_, value) => _.IsVisible = value
        );

    public bool IsVisible
    {
        get;
        set => SetAndRaise(IsVisibleProperty, ref field, value);
    }

    public static readonly DirectProperty<OldAttitudeIndicator.ScaleItem, string?> TitleProperty =
        AvaloniaProperty.RegisterDirect<OldAttitudeIndicator.ScaleItem, string?>(
            nameof(Title),
            _ => _.Title,
            (_, value) => _.Title = value
        );

    public string? Title
    {
        get;
        set => SetAndRaise(TitleProperty, ref field, value);
    }

    public static readonly DirectProperty<OldAttitudeIndicator.ScaleItem, double> ValueProperty =
        AvaloniaProperty.RegisterDirect<OldAttitudeIndicator.ScaleItem, double>(
            nameof(Value),
            _ => _.Value,
            (_, value) => _.Value = value
        );

    public double Value
    {
        get;
        set => SetAndRaise(ValueProperty, ref field, value);
    }

    public static readonly DirectProperty<OldAttitudeIndicator.ScaleItem, double> PositionProperty =
        AvaloniaProperty.RegisterDirect<OldAttitudeIndicator.ScaleItem, double>(
            nameof(Position),
            _ => _.Position,
            (_, value) => _.Position = value
        );

    public double Position
    {
        get;
        set => SetAndRaise(PositionProperty, ref field, value);
    }
}
