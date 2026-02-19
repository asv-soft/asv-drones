// TODO: asv-soft-u08

using System.Collections.Generic;
using Avalonia;

namespace Asv.Drones.AngleIndicator;

public partial class UavAngleIndicator
{
    public static readonly StyledProperty<double> RollAngleProperty = AvaloniaProperty.Register<
        UavAngleIndicator,
        double
    >(nameof(RollAngle));

    public double RollAngle
    {
        get => GetValue(RollAngleProperty);
        set => SetValue(RollAngleProperty, value);
    }

    public static readonly StyledProperty<double> PitchAngleProperty = AvaloniaProperty.Register<
        UavAngleIndicator,
        double
    >(nameof(PitchAngle));

    public double PitchAngle
    {
        get => GetValue(PitchAngleProperty);
        set => SetValue(PitchAngleProperty, value);
    }

    #region Internal direct property

    public static readonly DirectProperty<UavAngleIndicator, double> InternalWidthProperty =
        AvaloniaProperty.RegisterDirect<UavAngleIndicator, double>(
            nameof(InternalWidth),
            _ => _.InternalWidth,
            (_, value) => _.InternalWidth = value
        );

    public double InternalWidth
    {
        get;
        set => SetAndRaise(InternalWidthProperty, ref field, value);
    } = 1000;

    public static readonly DirectProperty<UavAngleIndicator, double> InternalHeightProperty =
        AvaloniaProperty.RegisterDirect<UavAngleIndicator, double>(
            nameof(InternalHeight),
            _ => _.InternalHeight,
            (_, value) => _.InternalHeight = value
        );

    public double InternalHeight
    {
        get;
        set => SetAndRaise(InternalHeightProperty, ref field, value);
    } = 1000;

    public static readonly DirectProperty<UavAngleIndicator, double> PitchTranslateXProperty =
        AvaloniaProperty.RegisterDirect<UavAngleIndicator, double>(
            nameof(PitchTranslateX),
            _ => _.PitchTranslateX,
            (_, value) => _.PitchTranslateX = value
        );

    private double PitchTranslateX
    {
        get;
        set => SetAndRaise(PitchTranslateXProperty, ref field, value);
    }

    public static readonly DirectProperty<UavAngleIndicator, double> PitchTranslateYProperty =
        AvaloniaProperty.RegisterDirect<UavAngleIndicator, double>(
            nameof(PitchTranslateY),
            _ => _.PitchTranslateY,
            (_, value) => _.PitchTranslateY = value
        );

    public double PitchTranslateY
    {
        get;
        set => SetAndRaise(PitchTranslateYProperty, ref field, value);
    }

    public static readonly DirectProperty<
        UavAngleIndicator,
        IEnumerable<RollItem>
    > RollItemsProperty = AvaloniaProperty.RegisterDirect<UavAngleIndicator, IEnumerable<RollItem>>(
        nameof(RollItems),
        _ => _.RollItems,
        (_, value) => _.RollItems = value
    );

    public IEnumerable<RollItem> RollItems
    {
        get;
        set => SetAndRaise(RollItemsProperty, ref field, value);
    }

    public static readonly DirectProperty<
        UavAngleIndicator,
        IEnumerable<PitchItem>
    > PitchItemsProperty = AvaloniaProperty.RegisterDirect<
        UavAngleIndicator,
        IEnumerable<PitchItem>
    >(nameof(PitchItems), _ => _.PitchItems, (_, value) => _.PitchItems = value);

    public IEnumerable<PitchItem> PitchItems
    {
        get;
        set => SetAndRaise(PitchItemsProperty, ref field, value);
    }
    #endregion
}