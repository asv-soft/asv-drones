using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Media;
using Material.Icons;

namespace Asv.Drones.Gui.Core;

[PseudoClasses(":critical", ":warning", ":normal", ":unknown")]
public class ConnectionQuality : IndicatorBase
{
    #region Brushes
   
    public static readonly StyledProperty<Brush> UnknownBrushProperty = AvaloniaProperty.Register<ConnectionQuality, Brush>(
        "UnknownBrush");

    public Brush UnknownBrush
    {
        get => GetValue(UnknownBrushProperty);
        set => SetValue(UnknownBrushProperty, value);
    }
    
    public static readonly StyledProperty<Brush> CriticalBrushProperty = AvaloniaProperty.Register<ConnectionQuality, Brush>(
        "CriticalBrush");

    public Brush CriticalBrush
    {
        get => GetValue(CriticalBrushProperty);
        set => SetValue(CriticalBrushProperty, value);
    }
    public static readonly StyledProperty<Brush> WarningBrushProperty = AvaloniaProperty.Register<ConnectionQuality, Brush>(
        "WarningBrush");

    public Brush WarningBrush
    {
        get => GetValue(WarningBrushProperty);
        set => SetValue(WarningBrushProperty, value);
    }
    public static readonly StyledProperty<Brush> NormalBrushProperty = AvaloniaProperty.Register<ConnectionQuality, Brush>(
        "NormalBrush");

    public Brush NormalBrush
    {
        get => GetValue(NormalBrushProperty);
        set => SetValue(NormalBrushProperty, value);
    }
    
    #endregion
    
    #region Styled Props
    
    public static readonly StyledProperty<double> CriticalValueProperty = AvaloniaProperty.Register<ConnectionQuality, double>(
        nameof(CriticalValue), 0.2, notifying: UpdateValue);

    public double CriticalValue
    {
        get => GetValue(CriticalValueProperty);
        set => SetValue(CriticalValueProperty, value);
    }
    
    public static readonly StyledProperty<double> WarningValueProperty = AvaloniaProperty.Register<ConnectionQuality, double>(
        nameof(WarningValue),0.5, notifying: UpdateValue);

    public double WarningValue
    {
        get => GetValue(WarningValueProperty);
        set => SetValue(WarningValueProperty, value);
    }
    
    public static readonly StyledProperty<double> MaxValueProperty = AvaloniaProperty.Register<ConnectionQuality, double>(
        nameof(MaxValue), 1, notifying: UpdateValue);
    
    public double MaxValue
    {
        get => GetValue(MaxValueProperty);
        set => SetValue(MaxValueProperty, value);
    }

    public static readonly StyledProperty<double?> ValueProperty = AvaloniaProperty.Register<ConnectionQuality, double?>(
        nameof(Value), default(double?), notifying: UpdateValue);

    public double? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly StyledProperty<MaterialIconKind> IconKindProperty = AvaloniaProperty.Register<ConnectionQuality, MaterialIconKind>(
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
    
    private static void UpdateValue(IAvaloniaObject source, bool beforeChanged)
    {
        if (source is not ConnectionQuality indicator) return;

        SetPseudoClass(indicator);
        
        indicator.IconKind = GetIcon(indicator.Value/indicator.MaxValue);
    }
    
    private static void SetPseudoClass(ConnectionQuality connection)
    {
        var value = connection.Value;        
        connection.PseudoClasses.Set(":unknown", value == null || double.IsFinite(value.Value) == false || value > connection.MaxValue);
        connection.PseudoClasses.Set(":critical", value <= connection.CriticalValue);
        connection.PseudoClasses.Set(":warning", value > connection.CriticalValue & value <= connection.WarningValue);
        connection.PseudoClasses.Set(":normal", value > connection.WarningValue & value <= connection.MaxValue);
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