using System.Reactive.Linq;
using Asv.Common;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using ReactiveUI;

namespace Asv.Drones.Gui.Core;

public class LevelIndicator : TemplatedControl
{
    public static readonly StyledProperty<double> ValueFromProperty = AvaloniaProperty.Register<LevelIndicator, double>(
        nameof(ValueFrom), 100, notifying: UpdateValueLimits);
    
    public double ValueFrom
    {
        get => GetValue(ValueFromProperty);
        set => SetValue(ValueFromProperty, value);
    }
    
    public static readonly StyledProperty<double> ValueToProperty = AvaloniaProperty.Register<LevelIndicator, double>(
        nameof(ValueTo), 100, notifying: UpdateValueLimits);
    
    public double ValueTo
    {
        get => GetValue(ValueToProperty);
        set => SetValue(ValueToProperty, value);
    }
    
    public static readonly StyledProperty<double?> LevelProperty = AvaloniaProperty.Register<LevelIndicator, double?>(
        nameof(Level));
    
    public double? Level
    {
        get => GetValue(LevelProperty);
        set => SetValue(LevelProperty, value);
    }

    public static readonly StyledProperty<double?> ValueProperty = AvaloniaProperty.Register<LevelIndicator, double?>(
        nameof(Value), default(double?), notifying: UpdateValue);

    public double? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
    
    private static void UpdateValueLimits(IAvaloniaObject source, bool beforeChanged)
    {
        if (source is not LevelIndicator indicator) return;

        if (indicator.ValueTo <= indicator.ValueFrom || double.IsFinite(indicator.ValueTo) == false || double.IsFinite(indicator.ValueTo) == false)
        {
            indicator.Value = indicator.ValueFrom;
        }
        else
        {
            UpdateValue(source, beforeChanged);
        }
    }

    private static void UpdateValue(IAvaloniaObject source, bool beforeChanged)
    {
        if (source is not LevelIndicator indicator) return;

        if (double.IsFinite(indicator.ValueTo) == false || double.IsFinite(indicator.ValueTo) == false)
        {
            indicator.Level = 0;
            return;
        }

        if (indicator.Value < indicator.ValueFrom)
        {
            indicator.Level = 0;
            return;
        }
        
        if (indicator.Value > indicator.ValueTo)
        {
            indicator.Level = 350;
            return;
        }

        indicator.Level = (indicator.Value - indicator.ValueFrom) / (indicator.ValueTo - indicator.ValueFrom) * 350.0;
    }

    private Random _random = new Random();
    public LevelIndicator()
    {
        // if (Design.IsDesignMode)
        // {
        //     Width = 350;
        //     Height = 41;
        //     Foreground = Brushes.DarkBlue;
        //     BorderBrush = Brushes.Transparent;
        //     BorderThickness = new Thickness(0);
        //
        //     ValueFrom = -120;
        //     ValueTo = 20;
        //     Value = -60;
        //
        //     Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1),RxApp.MainThreadScheduler).Subscribe(_ =>
        //     {
        //         Value = _random.NextDouble() * 140 + ValueFrom;
        //     });
        // }
        

    }
}