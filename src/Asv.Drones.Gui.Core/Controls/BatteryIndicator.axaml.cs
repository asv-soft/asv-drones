using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Material.Icons;

namespace Asv.Drones.Gui.Core;

public class BatteryIndicator : TemplatedControl
{
    #region Privates
    private readonly SolidColorBrush _criticalValueColor = new SolidColorBrush(Color.FromRgb(153, 24, 24));
    private readonly SolidColorBrush _warningValueColor = new SolidColorBrush(Color.FromRgb(196, 103, 22));
    private readonly SolidColorBrush _maxValueColor = new SolidColorBrush(Color.FromRgb(101, 150, 21));
    #endregion
    
    #region Styled Props
    public static readonly StyledProperty<double> CriticalValueProperty = AvaloniaProperty.Register<BatteryIndicator, double>(
        nameof(CriticalValue), 20);

    public double CriticalValue
    {
        get => GetValue(CriticalValueProperty);
        set => SetValue(CriticalValueProperty, value);
    }

    public static readonly StyledProperty<double> WarningValueProperty = AvaloniaProperty.Register<BatteryIndicator, double>(
        nameof(WarningValue),50);

    public double WarningValue
    {
        get => GetValue(WarningValueProperty);
        set => SetValue(WarningValueProperty, value);
    }

    public static readonly StyledProperty<double> MaxValueProperty = AvaloniaProperty.Register<BatteryIndicator, double>(
        nameof(MaxValue), 100);

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

    private static readonly StyledProperty<MaterialIconKind> IconKindProperty = AvaloniaProperty.Register<BatteryIndicator, MaterialIconKind>(
        nameof(IconKind), MaterialIconKind.BatteryUnknown);

    private MaterialIconKind IconKind
    {
        get => GetValue(IconKindProperty);
        set => SetValue(IconKindProperty, value);
    }

    private static readonly StyledProperty<SolidColorBrush> IconColorProperty = AvaloniaProperty.Register<BatteryIndicator, SolidColorBrush>(
        nameof(IconColor));

    private SolidColorBrush IconColor
    {
        get => GetValue(IconColorProperty);
        set => SetValue(IconColorProperty, value);
    }
    #endregion

    public BatteryIndicator()
    {
        
    }

    private static void UpdateValue(IAvaloniaObject source, bool beforeChanged)
    {
        if (source is not BatteryIndicator indicator) return;

        indicator.IconKind = GetIcon(indicator.Value);
        
        if (indicator.Value > indicator.WarningValue & indicator.Value <= indicator.MaxValue)
        {
            indicator.IconColor = indicator._maxValueColor;
        }
        else if (indicator.Value > indicator.CriticalValue & indicator.Value <= indicator.WarningValue)
        {
            indicator.IconColor = indicator._warningValueColor;
        }
        else if (indicator.Value <= indicator.CriticalValue)
        {
            indicator.IconColor = indicator._criticalValueColor;
        }
    }

    private static MaterialIconKind GetIcon(double? value)
    {
        return (value ?? default(double)) switch
        {
            (< 0 or Double.NegativeInfinity 
                 or Double.PositiveInfinity 
                 or Double.NaN) => MaterialIconKind.BatteryUnknown,
            (0) => MaterialIconKind.Battery0,
            (> 0 and <= 10) => MaterialIconKind.Battery10,
            (> 10 and <= 20) => MaterialIconKind.Battery20,
            (> 20 and <= 30) => MaterialIconKind.Battery30,
            (> 30 and <= 40) => MaterialIconKind.Battery40,
            (> 40 and <= 50) => MaterialIconKind.Battery50,
            (> 50 and <= 60) => MaterialIconKind.Battery60,
            (> 60 and <= 70) => MaterialIconKind.Battery70,
            (> 70 and <= 80) => MaterialIconKind.Battery80,
            (> 80 and <= 90) => MaterialIconKind.Battery90,
            (> 90 and <= 100) => MaterialIconKind.Battery100
        };
    }
    
    
}