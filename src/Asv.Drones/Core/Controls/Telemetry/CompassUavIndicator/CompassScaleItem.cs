using System;
using Avalonia;

namespace Asv.Drones;

public class CompassScaleItem : AvaloniaObject
{
    private const double Center = 150.0;
    private const double OuterRadius = 128.0;
    private const double LabelRadius = 88.0;
    private const double LabelWidth = 44.0;
    private const double LabelHeight = 24.0;

    public CompassScaleItem(double angle, string? title, bool isMajor)
    {
        Angle = angle;
        Title = title;
        HasTitle = !string.IsNullOrWhiteSpace(title);
        TickLength = isMajor ? 20 : 10;
        TickWidth = isMajor ? 3 : 2;
        Update(0);
    }

    public double Angle { get; }
    public string? Title { get; }
    public bool HasTitle { get; }
    public double TickLength { get; }
    public double TickWidth { get; }

    public static readonly DirectProperty<CompassScaleItem, Point> TickStartProperty =
        AvaloniaProperty.RegisterDirect<CompassScaleItem, Point>(
            nameof(TickStart),
            item => item.TickStart,
            (item, value) => item.TickStart = value
        );

    public Point TickStart
    {
        get;
        private set => SetAndRaise(TickStartProperty, ref field, value);
    }

    public static readonly DirectProperty<CompassScaleItem, Point> TickEndProperty =
        AvaloniaProperty.RegisterDirect<CompassScaleItem, Point>(
            nameof(TickEnd),
            item => item.TickEnd,
            (item, value) => item.TickEnd = value
        );

    public Point TickEnd
    {
        get;
        private set => SetAndRaise(TickEndProperty, ref field, value);
    }

    public static readonly DirectProperty<CompassScaleItem, double> LabelLeftProperty =
        AvaloniaProperty.RegisterDirect<CompassScaleItem, double>(
            nameof(LabelLeft),
            item => item.LabelLeft,
            (item, value) => item.LabelLeft = value
        );

    public double LabelLeft
    {
        get;
        private set => SetAndRaise(LabelLeftProperty, ref field, value);
    }

    public static readonly DirectProperty<CompassScaleItem, double> LabelTopProperty =
        AvaloniaProperty.RegisterDirect<CompassScaleItem, double>(
            nameof(LabelTop),
            item => item.LabelTop,
            (item, value) => item.LabelTop = value
        );

    public double LabelTop
    {
        get;
        private set => SetAndRaise(LabelTopProperty, ref field, value);
    }

    public void Update(double heading)
    {
        var visualAngle = NormalizeSignedAngle(Angle - heading);
        var radians = visualAngle * Math.PI / 180.0;
        var sin = Math.Sin(radians);
        var cos = Math.Cos(radians);

        TickStart = new Point(
            Center + ((OuterRadius - TickLength) * sin),
            Center - ((OuterRadius - TickLength) * cos)
        );
        TickEnd = new Point(Center + (OuterRadius * sin), Center - (OuterRadius * cos));
        LabelLeft = Center + (LabelRadius * sin) - (LabelWidth / 2.0);
        LabelTop = Center - (LabelRadius * cos) - (LabelHeight / 2.0);
    }

    private static double NormalizeSignedAngle(double value)
    {
        var angle = value % 360.0;
        if (angle <= -180.0)
        {
            angle += 360.0;
        }
        else if (angle > 180.0)
        {
            angle -= 360.0;
        }

        return angle;
    }
}
