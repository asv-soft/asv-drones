using System;
using Asv.Avalonia;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;

namespace Asv.Drones;

[PseudoClasses(ProgressDisabledPseudoclass, ProgressCompletedPseudoclass)]
public class RouteUavIndicator : IndicatorBase
{
    private double _internalBorderWidth;

    public static readonly DirectProperty<RouteUavIndicator, double> InternalBorderWidthProperty =
        AvaloniaProperty.RegisterDirect<RouteUavIndicator, double>(
            nameof(InternalBorderWidth),
            o => o.InternalBorderWidth,
            (o, v) => o.InternalBorderWidth = v
        );

    public double InternalBorderWidth
    {
        get => _internalBorderWidth;
        private set => SetAndRaise(InternalBorderWidthProperty, ref _internalBorderWidth, value);
    }

    private double _internalBorderLeft;

    public static readonly DirectProperty<RouteUavIndicator, double> InternalBorderLeftProperty =
        AvaloniaProperty.RegisterDirect<RouteUavIndicator, double>(
            nameof(InternalBorderLeft),
            o => o.InternalBorderLeft,
            (o, v) => o.InternalBorderLeft = v
        );

    public double InternalBorderLeft
    {
        get => _internalBorderLeft;
        set => SetAndRaise(InternalBorderLeftProperty, ref _internalBorderLeft, value);
    }

    public static readonly StyledProperty<double> ProgressProperty = AvaloniaProperty.Register<
        RouteUavIndicator,
        double
    >(nameof(Progress));

    public double Progress
    {
        get => GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    private double _internalIndicatorLeft;

    public static readonly DirectProperty<RouteUavIndicator, double> InternalIndicatorLeftProperty =
        AvaloniaProperty.RegisterDirect<RouteUavIndicator, double>(
            nameof(InternalIndicatorLeft),
            o => o.InternalIndicatorLeft,
            (o, v) => o.InternalIndicatorLeft = v
        );

    public double InternalIndicatorLeft
    {
        get => _internalIndicatorLeft;
        set => SetAndRaise(InternalIndicatorLeftProperty, ref _internalIndicatorLeft, value);
    }

    private string _internalProgressText;

    public static readonly DirectProperty<RouteUavIndicator, string> InternalProgressTextProperty =
        AvaloniaProperty.RegisterDirect<RouteUavIndicator, string>(
            nameof(InternalProgressText),
            o => o.InternalProgressText,
            (o, v) => o.InternalProgressText = v
        );

    public string InternalProgressText
    {
        get => _internalProgressText;
        set => SetAndRaise(InternalProgressTextProperty, ref _internalProgressText, value);
    }

    public static readonly StyledProperty<string> StatusTextProperty = AvaloniaProperty.Register<
        RouteUavIndicator,
        string
    >(nameof(StatusText));

    public string StatusText
    {
        get => GetValue(StatusTextProperty);
        set => SetValue(StatusTextProperty, value);
    }

    public static readonly StyledProperty<string> SubStatusTextProperty = AvaloniaProperty.Register<
        RouteUavIndicator,
        string
    >(nameof(SubStatusText));

    public string SubStatusText
    {
        get => GetValue(SubStatusTextProperty);
        set => SetValue(SubStatusTextProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ProgressProperty)
        {
            var progress = (double)change.NewValue!;

            if (double.IsNaN(progress))
            {
                InternalProgressText = string.Empty;
                PseudoClasses.Add(ProgressDisabledPseudoclass);
                return;
            }

            PseudoClasses.Remove(ProgressDisabledPseudoclass);
            PseudoClasses.Set(ProgressCompletedPseudoclass, Math.Abs(Progress - 1.0) < 0.01);

            InternalProgressText = $"{progress * 100.0:F0} %";

            if (progress <= 0.0)
            {
                progress = 0.0000001;
            }

            if (progress >= 1.0)
            {
                progress = 0.9999999;
            }

            InternalIndicatorLeft = 20 + (progress * 690.0);
            InternalBorderLeft = progress * 690.0;
            InternalBorderWidth = 800.0 - InternalBorderLeft;
        }
    }
}
