using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Material.Icons;

namespace Asv.Drones.Gui.Core;

[PseudoClasses(":critical", ":warning", ":normal", ":unknown")]
public class BatteryIndicator : IndicatorBase
{
    #region Styled Props
    
    public static readonly StyledProperty<double> CriticalValueProperty = AvaloniaProperty.Register<BatteryIndicator, double>(
        nameof(CriticalValue), 20, notifying: UpdateValue);

    public double CriticalValue
    {
        get => GetValue(CriticalValueProperty);
        set => SetValue(CriticalValueProperty, value);
    }
    
    public static readonly StyledProperty<double> WarningValueProperty = AvaloniaProperty.Register<BatteryIndicator, double>(
        nameof(WarningValue),50, notifying: UpdateValue);

    public double WarningValue
    {
        get => GetValue(WarningValueProperty);
        set => SetValue(WarningValueProperty, value);
    }
    
    public static readonly StyledProperty<double> MaxValueProperty = AvaloniaProperty.Register<BatteryIndicator, double>(
        nameof(MaxValue), 100, notifying: UpdateValue);
    
    public double MaxValue
    {
        get => GetValue(MaxValueProperty);
        set => SetValue(MaxValueProperty, value);
    }

    public static readonly StyledProperty<double?> ValueProperty = AvaloniaProperty.Register<BatteryIndicator, double?>(
        nameof(Value), default(double?), notifying: UpdateValue);

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
        var value = indicator.Value;        
        indicator.PseudoClasses.Set(":unknown", value == null || double.IsFinite(value.Value) == false || value > indicator.MaxValue);
        indicator.PseudoClasses.Set(":critical", value <= indicator.CriticalValue);
        indicator.PseudoClasses.Set(":warning", value > indicator.CriticalValue & value <= indicator.WarningValue);
        indicator.PseudoClasses.Set(":normal", value > indicator.WarningValue & value <= indicator.MaxValue);
    }
    
    private static void UpdateValue(IAvaloniaObject source, bool beforeChanged)
    {
        if (source is not BatteryIndicator indicator) return;

        SetPseudoClass(indicator);
        if (indicator.MaxValue == 0 || double.IsFinite(indicator.MaxValue) == false)
        {
            
        }
        indicator.IconKind = GetIcon(indicator.Value/indicator.MaxValue);
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