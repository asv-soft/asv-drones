using Avalonia;

namespace Asv.Drones;

public partial class ScaleItem
{
    private readonly double _valueRange;
    private readonly bool _showNegative;
    private readonly double _startPosition;
    private readonly double _positionStep;
    private readonly double _valueOffset;
    private readonly bool _isFixedTitle;
    private string? _title;
    private double _value;
    private double _position;
    private bool _isVisible;

    public static readonly DirectProperty<ScaleItem, bool> IsVisibleProperty =
        AvaloniaProperty.RegisterDirect<ScaleItem, bool>(
            nameof(IsVisible),
            _ => _.IsVisible,
            (_, value) => _.IsVisible = value
        );

    public bool IsVisible
    {
        get => _isVisible;
        set => SetAndRaise(IsVisibleProperty, ref _isVisible, value);
    }

    public static readonly DirectProperty<ScaleItem, string?> TitleProperty =
        AvaloniaProperty.RegisterDirect<ScaleItem, string?>(
            nameof(Title),
            _ => _.Title,
            (_, value) => _.Title = value
        );

    public string? Title
    {
        get => _title;
        set => SetAndRaise(TitleProperty, ref _title, value);
    }

    public static readonly DirectProperty<ScaleItem, double> ValueProperty =
        AvaloniaProperty.RegisterDirect<ScaleItem, double>(
            nameof(Value),
            _ => _.Value,
            (_, value) => _.Value = value
        );

    public double Value
    {
        get => _value;
        set => SetAndRaise(ValueProperty, ref _value, value);
    }

    public static readonly DirectProperty<ScaleItem, double> PositionProperty =
        AvaloniaProperty.RegisterDirect<ScaleItem, double>(
            nameof(Position),
            _ => _.Position,
            (_, value) => _.Position = value
        );

    public double Position
    {
        get => _position;
        set => SetAndRaise(PositionProperty, ref _position, value);
    }
}
