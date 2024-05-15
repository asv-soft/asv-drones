using Avalonia;
using Avalonia.Controls.Primitives;

namespace Asv.Drones.Gui.Api;

public class LevelIndicator : TemplatedControl
{
    public static readonly StyledProperty<double> ValueFromProperty = AvaloniaProperty.Register<LevelIndicator, double>(
        nameof(ValueFrom), 100);

    public double ValueFrom
    {
        get => GetValue(ValueFromProperty);
        set => SetValue(ValueFromProperty, value);
    }

    public static readonly StyledProperty<double> ValueToProperty = AvaloniaProperty.Register<LevelIndicator, double>(
        nameof(ValueTo), 100);

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
        nameof(Value), default(double?));

    public double? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    private static void UpdateValueLimits(AvaloniaObject source)
    {
        if (source is not LevelIndicator indicator) return;

        if (indicator.ValueTo <= indicator.ValueFrom || double.IsFinite(indicator.ValueTo) == false ||
            double.IsFinite(indicator.ValueTo) == false)
        {
            indicator.Value = indicator.ValueFrom;
        }
        else
        {
            UpdateValue(source);
        }
    }

    private static void UpdateValue(AvaloniaObject source)
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

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ValueFromProperty || change.Property == ValueToProperty)
        {
            UpdateValueLimits(change.Sender);
        }
        else if (change.Property == ValueProperty)
        {
            UpdateValue(change.Sender);
        }
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