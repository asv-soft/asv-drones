using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Material.Icons;

namespace Asv.Drones.Gui.Api;

[PseudoClasses(":critical", ":warning", ":normal", ":unknown")]
public class BatteryIndicator : IndicatorBase
{
    #region Styled Props

    public static readonly StyledProperty<double> CriticalValueProperty =
        AvaloniaProperty.Register<BatteryIndicator, double>(
            nameof(CriticalValue), 20);

    public double CriticalValue
    {
        get => GetValue(CriticalValueProperty);
        set => SetValue(CriticalValueProperty, value);
    }

    public static readonly StyledProperty<double> WarningValueProperty =
        AvaloniaProperty.Register<BatteryIndicator, double>(
            nameof(WarningValue), 50);

    public double WarningValue
    {
        get => GetValue(WarningValueProperty);
        set => SetValue(WarningValueProperty, value);
    }

    public static readonly StyledProperty<double> MaxValueProperty =
        AvaloniaProperty.Register<BatteryIndicator, double>(
            nameof(MaxValue), 100);

    public double MaxValue
    {
        get => GetValue(MaxValueProperty);
        set => SetValue(MaxValueProperty, value);
    }

    public static readonly StyledProperty<double?> ValueProperty = AvaloniaProperty.Register<BatteryIndicator, double?>(
        nameof(Value), default(double?));

    public double? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    #endregion

    public BatteryIndicator()
    {
    }

    private static void SetPseudoClass(BatteryIndicator indicator)
    {
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ValueProperty || change.Property == MaxValueProperty ||
            change.Property == CriticalValueProperty || change.Property == WarningValueProperty)
        {
            var value = Value;
            PseudoClasses.Set(":unknown", value == null || double.IsFinite(value.Value) == false || value > MaxValue);
            PseudoClasses.Set(":critical", value <= CriticalValue);
            PseudoClasses.Set(":warning", value > CriticalValue & value <= WarningValue);
            PseudoClasses.Set(":normal", value > WarningValue & value <= MaxValue);
            if (MaxValue == 0 || double.IsFinite(MaxValue) == false)
            {
            }

            IconKind = GetIcon(Value / MaxValue);
        }
    }

    private static MaterialIconKind GetIcon(double? normalizedValue)
    {
        return (normalizedValue ?? double.NaN) switch
        {
            (< 0 or > 1
                or double.NegativeInfinity
                or double.PositiveInfinity
                or double.NaN) => MaterialIconKind.BatteryUnknown,
            (0) => MaterialIconKind.Battery0,
            (> 0 and <= 0.10) => MaterialIconKind.Battery10,
            (> 0.10 and <= 0.20) => MaterialIconKind.Battery20,
            (> 0.20 and <= 0.30) => MaterialIconKind.Battery30,
            (> 0.30 and <= 0.40) => MaterialIconKind.Battery40,
            (> 0.40 and <= 0.50) => MaterialIconKind.Battery50,
            (> 0.50 and <= 0.60) => MaterialIconKind.Battery60,
            (> 0.60 and <= 0.70) => MaterialIconKind.Battery70,
            (> 0.70 and <= 0.80) => MaterialIconKind.Battery80,
            (> 0.80 and <= 0.90) => MaterialIconKind.Battery90,
            (> 0.90 and <= 1) => MaterialIconKind.Battery100
        };
    }
}