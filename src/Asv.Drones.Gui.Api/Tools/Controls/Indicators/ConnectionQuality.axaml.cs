using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Material.Icons;

namespace Asv.Drones.Gui.Api;

[PseudoClasses(":critical", ":warning", ":normal", ":unknown")]
public class ConnectionQuality : IndicatorBase
{
    #region Styled Props

    public static readonly StyledProperty<double> CriticalValueProperty =
        AvaloniaProperty.Register<ConnectionQuality, double>(
            nameof(CriticalValue), 0.2);

    public double CriticalValue
    {
        get => GetValue(CriticalValueProperty);
        set => SetValue(CriticalValueProperty, value);
    }

    public static readonly StyledProperty<double> WarningValueProperty =
        AvaloniaProperty.Register<ConnectionQuality, double>(
            nameof(WarningValue), 0.5);

    public double WarningValue
    {
        get => GetValue(WarningValueProperty);
        set => SetValue(WarningValueProperty, value);
    }

    public static readonly StyledProperty<double> MaxValueProperty =
        AvaloniaProperty.Register<ConnectionQuality, double>(
            nameof(MaxValue), 1);

    public double MaxValue
    {
        get => GetValue(MaxValueProperty);
        set => SetValue(MaxValueProperty, value);
    }

    public static readonly StyledProperty<double?> ValueProperty =
        AvaloniaProperty.Register<ConnectionQuality, double?>(
            nameof(Value), default(double?));

    public double? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly StyledProperty<MaterialIconKind> IconKindProperty =
        AvaloniaProperty.Register<ConnectionQuality, MaterialIconKind>(
            nameof(IconKind), MaterialIconKind.WifiStrengthAlertOutline);

    public MaterialIconKind IconKind
    {
        get => GetValue(IconKindProperty);
        set => SetValue(IconKindProperty, value);
    }

    #endregion

    public ConnectionQuality()
    {
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ValueProperty)
        {
            var value = Value;
            PseudoClasses.Set(":unknown", value == null || double.IsFinite(value.Value) == false || value > MaxValue);
            PseudoClasses.Set(":critical", value <= CriticalValue);
            PseudoClasses.Set(":warning", value > CriticalValue & value <= WarningValue);
            PseudoClasses.Set(":normal", value > WarningValue & value <= MaxValue);
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
                or double.NaN) => MaterialIconKind.WifiStrengthAlertOutline,
            (0) => MaterialIconKind.WifiStrength0,
            (> 0 and <= 0.2) => MaterialIconKind.WifiStrength0,
            (> 0.2 and <= 0.4) => MaterialIconKind.WifiStrength1,
            (> 0.4 and <= 0.6) => MaterialIconKind.WifiStrength2,
            (> 0.6 and <= 0.8) => MaterialIconKind.WifiStrength3,
            (> 0.8 and <= 1.0) => MaterialIconKind.WifiStrength4,
        };
    }
}