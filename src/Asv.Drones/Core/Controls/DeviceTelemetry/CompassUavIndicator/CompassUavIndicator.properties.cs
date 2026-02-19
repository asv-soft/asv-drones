using System.Collections.Generic;
using Avalonia;

namespace Asv.Drones;

public partial class CompassUavIndicator
{
    public static readonly StyledProperty<double> HeadingProperty = AvaloniaProperty.Register<
        CompassUavIndicator,
        double
    >(nameof(Heading));

    public double Heading
    {
        get => GetValue(HeadingProperty);
        set => SetValue(HeadingProperty, value);
    }

    public static readonly StyledProperty<double> HomeAzimuthProperty = AvaloniaProperty.Register<
        CompassUavIndicator,
        double
    >(nameof(HomeAzimuth), double.NaN);

    public double HomeAzimuth
    {
        get => GetValue(HomeAzimuthProperty);
        set => SetValue(HomeAzimuthProperty, value);
    }

    public static readonly DirectProperty<
        CompassUavIndicator,
        IEnumerable<CompassScaleItem>
    > CompassItemsProperty = AvaloniaProperty.RegisterDirect<
        CompassUavIndicator,
        IEnumerable<CompassScaleItem>
    >(nameof(CompassItems), indicator => indicator.CompassItems);

    public IEnumerable<CompassScaleItem> CompassItems { get; }

    public static readonly DirectProperty<CompassUavIndicator, string> HeadingTextProperty =
        AvaloniaProperty.RegisterDirect<CompassUavIndicator, string>(
            nameof(HeadingText),
            indicator => indicator.HeadingText,
            (indicator, value) => indicator.HeadingText = value
        );

    public string HeadingText
    {
        get;
        private set => SetAndRaise(HeadingTextProperty, ref field, value);
    } = "0°";

    public static readonly DirectProperty<CompassUavIndicator, double> HomeMarkerRotationProperty =
        AvaloniaProperty.RegisterDirect<CompassUavIndicator, double>(
            nameof(HomeMarkerRotation),
            indicator => indicator.HomeMarkerRotation,
            (indicator, value) => indicator.HomeMarkerRotation = value
        );

    public double HomeMarkerRotation
    {
        get;
        private set => SetAndRaise(HomeMarkerRotationProperty, ref field, value);
    }
}
