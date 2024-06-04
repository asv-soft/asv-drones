using Avalonia;

namespace Asv.Drones.Gui.Api;

public class DigitIndicator : IndicatorBase
{
    #region Direct properties

    private string _title;

    public static readonly DirectProperty<DigitIndicator, string> TitleProperty =
        AvaloniaProperty.RegisterDirect<DigitIndicator, string>(
            nameof(Title), o => o.Title, (o, v) => o.Title = v);

    public string Title
    {
        get => _title;
        set => SetAndRaise(TitleProperty, ref _title, value);
    }

    private string _units;

    public static readonly DirectProperty<DigitIndicator, string> UnitsProperty =
        AvaloniaProperty.RegisterDirect<DigitIndicator, string>(
            nameof(Units), o => o.Units, (o, v) => o.Units = v);

    public string Units
    {
        get => _units;
        set => SetAndRaise(UnitsProperty, ref _units, value);
    }

    private double _value;

    public static readonly DirectProperty<DigitIndicator, double> ValueProperty =
        AvaloniaProperty.RegisterDirect<DigitIndicator, double>(
            nameof(Value), o => o.Value, (o, v) => o.Value = v);

    public double Value
    {
        get => _value;
        set
        {
            if (Math.Abs(value - _value) < double.Epsilon)
            {
                IsDecreased = false;
                IsIncreased = false;
            }
            else if (value > _value)
            {
                IsDecreased = false;
                IsIncreased = true;
            }
            else if (value < _value)
            {
                IsDecreased = true;
                IsIncreased = false;
            }

            SetAndRaise(ValueProperty, ref _value, value);
            FormatedValue = Value.ToString(FormatString);
        }
    }

    private string _formatString;

    public static readonly DirectProperty<DigitIndicator, string> FormatStringProperty =
        AvaloniaProperty.RegisterDirect<DigitIndicator, string>(
            nameof(FormatString), o => o.FormatString, (o, v) => o.FormatString = v);

    public string FormatString
    {
        get => _formatString;
        set => SetAndRaise(FormatStringProperty, ref _formatString, value);
    }

    private bool _isIncreased;

    public static readonly DirectProperty<DigitIndicator, bool> IsIncreasedProperty =
        AvaloniaProperty.RegisterDirect<DigitIndicator, bool>(
            nameof(IsIncreased), o => o.IsIncreased, (o, v) => o.IsIncreased = v);

    public bool IsIncreased
    {
        get => _isIncreased;
        set => SetAndRaise(IsIncreasedProperty, ref _isIncreased, value);
    }

    private bool _isDecreased;

    public static readonly DirectProperty<DigitIndicator, bool> IsDecreasedProperty =
        AvaloniaProperty.RegisterDirect<DigitIndicator, bool>(
            nameof(IsDecreased), o => o.IsDecreased, (o, v) => o.IsDecreased = v);

    public bool IsDecreased
    {
        get => _isDecreased;
        set => SetAndRaise(IsDecreasedProperty, ref _isDecreased, value);
    }

    #endregion

    private string _formatedValue;

    public static readonly DirectProperty<DigitIndicator, string> FormatedValueProperty =
        AvaloniaProperty.RegisterDirect<DigitIndicator, string>(
            nameof(FormatedValue), o => o.FormatedValue, (o, v) => o.FormatedValue = v);

    public string FormatedValue
    {
        get => _formatedValue;
        set => SetAndRaise(FormatedValueProperty, ref _formatedValue, value);
    }

    public DigitIndicator()
    {
        FormatString = "";
    }
}