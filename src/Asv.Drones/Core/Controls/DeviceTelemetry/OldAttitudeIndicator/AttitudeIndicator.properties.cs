using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;

namespace Asv.Drones.OldAttitudeIndicator;

public partial class AttitudeIndicator
{
    public static readonly DirectProperty<
        OldAttitudeIndicator.AttitudeIndicator,
        Color
    > BrushVibrationXProperty = AvaloniaProperty.RegisterDirect<
        OldAttitudeIndicator.AttitudeIndicator,
        Color
    >(nameof(BrushVibrationX), o => o.BrushVibrationX, (o, v) => o.BrushVibrationX = v);

    public Color BrushVibrationX
    {
        get;
        set => SetAndRaise(BrushVibrationXProperty, ref field, value);
    }

    public static readonly DirectProperty<
        OldAttitudeIndicator.AttitudeIndicator,
        Color
    > BrushVibrationYProperty = AvaloniaProperty.RegisterDirect<
        OldAttitudeIndicator.AttitudeIndicator,
        Color
    >(nameof(BrushVibrationY), o => o.BrushVibrationY, (o, v) => o.BrushVibrationY = v);

    public Color BrushVibrationY
    {
        get;
        set => SetAndRaise(BrushVibrationYProperty, ref field, value);
    }

    public static readonly DirectProperty<
        OldAttitudeIndicator.AttitudeIndicator,
        Color
    > BrushVibrationZProperty = AvaloniaProperty.RegisterDirect<
        OldAttitudeIndicator.AttitudeIndicator,
        Color
    >(nameof(BrushVibrationZ), o => o.BrushVibrationZ, (o, v) => o.BrushVibrationZ = v);

    public Color BrushVibrationZ
    {
        get;
        set => SetAndRaise(BrushVibrationZProperty, ref field, value);
    }

    public static readonly StyledProperty<float> VibrationXProperty = AvaloniaProperty.Register<
        OldAttitudeIndicator.AttitudeIndicator,
        float
    >(nameof(VibrationX), defaultValue: -1);

    public float VibrationX
    {
        get => GetValue(VibrationXProperty);
        set => SetValue(VibrationXProperty, value);
    }

    public static readonly StyledProperty<float> VibrationYProperty = AvaloniaProperty.Register<
        OldAttitudeIndicator.AttitudeIndicator,
        float
    >(nameof(VibrationY), defaultValue: -1);

    public float VibrationY
    {
        get => GetValue(VibrationYProperty);
        set => SetValue(VibrationYProperty, value);
    }

    public static readonly StyledProperty<float> VibrationZProperty = AvaloniaProperty.Register<
        OldAttitudeIndicator.AttitudeIndicator,
        float
    >(nameof(VibrationZ), defaultValue: -1);

    public float VibrationZ
    {
        get => GetValue(VibrationZProperty);
        set => SetValue(VibrationZProperty, value);
    }

    public static readonly StyledProperty<uint> Clipping0Property = AvaloniaProperty.Register<
        OldAttitudeIndicator.AttitudeIndicator,
        uint
    >(nameof(Clipping0));

    public uint Clipping0
    {
        get => GetValue(Clipping0Property);
        set => SetValue(Clipping0Property, value);
    }

    public static readonly StyledProperty<uint> Clipping1Property = AvaloniaProperty.Register<
        OldAttitudeIndicator.AttitudeIndicator,
        uint
    >(nameof(Clipping1));

    public uint Clipping1
    {
        get => GetValue(Clipping1Property);
        set => SetValue(Clipping1Property, value);
    }

    public static readonly StyledProperty<uint> Clipping2Property = AvaloniaProperty.Register<
        OldAttitudeIndicator.AttitudeIndicator,
        uint
    >(nameof(Clipping2));

    public uint Clipping2
    {
        get => GetValue(Clipping2Property);
        set => SetValue(Clipping2Property, value);
    }

    public static readonly StyledProperty<double> RollAngleProperty = AvaloniaProperty.Register<
        OldAttitudeIndicator.AttitudeIndicator,
        double
    >(nameof(RollAngle));

    public double RollAngle
    {
        get => GetValue(RollAngleProperty);
        set => SetValue(RollAngleProperty, value);
    }

    public static readonly StyledProperty<double> PitchAngleProperty = AvaloniaProperty.Register<
        OldAttitudeIndicator.AttitudeIndicator,
        double
    >(nameof(PitchAngle));

    public double PitchAngle
    {
        get => GetValue(PitchAngleProperty);
        set => SetValue(PitchAngleProperty, value);
    }

    public static readonly StyledProperty<double> VelocityProperty = AvaloniaProperty.Register<
        OldAttitudeIndicator.AttitudeIndicator,
        double
    >(nameof(Velocity));

    public double Velocity
    {
        get => GetValue(VelocityProperty);
        set => SetValue(VelocityProperty, value);
    }

    public static readonly StyledProperty<double> AltitudeProperty = AvaloniaProperty.Register<
        OldAttitudeIndicator.AttitudeIndicator,
        double
    >(nameof(Altitude));

    public double Altitude
    {
        get => GetValue(AltitudeProperty);
        set => SetValue(AltitudeProperty, value);
    }

    public static readonly StyledProperty<double> HeadingProperty = AvaloniaProperty.Register<
        OldAttitudeIndicator.AttitudeIndicator,
        double
    >(nameof(Heading));

    public double Heading
    {
        get => GetValue(HeadingProperty);
        set => SetValue(HeadingProperty, value);
    }

    public static readonly StyledProperty<double> HomeAzimuthProperty = AvaloniaProperty.Register<
        OldAttitudeIndicator.AttitudeIndicator,
        double
    >(nameof(HomeAzimuth));

    public double HomeAzimuth
    {
        get => GetValue(HomeAzimuthProperty);
        set => SetValue(HomeAzimuthProperty, value);
    }

    public static readonly StyledProperty<bool> IsArmedProperty = AvaloniaProperty.Register<
        OldAttitudeIndicator.AttitudeIndicator,
        bool
    >(nameof(IsArmed));

    public bool IsArmed
    {
        get => GetValue(IsArmedProperty);
        set => SetValue(IsArmedProperty, value);
    }

    public static readonly DirectProperty<
        OldAttitudeIndicator.AttitudeIndicator,
        string
    > StatusTextProperty = AvaloniaProperty.RegisterDirect<
        OldAttitudeIndicator.AttitudeIndicator,
        string
    >(nameof(StatusText), _ => _.StatusText, (_, value) => _.StatusText = value);

    public string StatusText
    {
        get;
        set => SetAndRaise(StatusTextProperty, ref field, value);
    }

    public static readonly DirectProperty<
        OldAttitudeIndicator.AttitudeIndicator,
        string
    > RightStatusTextProperty = AvaloniaProperty.RegisterDirect<
        OldAttitudeIndicator.AttitudeIndicator,
        string
    >(nameof(RightStatusText), _ => _.RightStatusText, (_, value) => _.RightStatusText = value);

    public string RightStatusText
    {
        get;
        set => SetAndRaise(RightStatusTextProperty, ref field, value);
    }

    public static readonly StyledProperty<TimeSpan> ArmedTimeProperty = AvaloniaProperty.Register<
        OldAttitudeIndicator.AttitudeIndicator,
        TimeSpan
    >(nameof(ArmedTime));

    public TimeSpan ArmedTime
    {
        get => GetValue(ArmedTimeProperty);
        set => SetValue(ArmedTimeProperty, value);
    }

    #region Internal direct property

    public static readonly DirectProperty<
        OldAttitudeIndicator.AttitudeIndicator,
        double
    > InternalWidthProperty = AvaloniaProperty.RegisterDirect<
        OldAttitudeIndicator.AttitudeIndicator,
        double
    >(nameof(InternalWidth), _ => _.InternalWidth, (_, value) => _.InternalWidth = value);

    public double InternalWidth
    {
        get;
        set => SetAndRaise(InternalWidthProperty, ref field, value);
    } = 1000;

    public static readonly DirectProperty<
        OldAttitudeIndicator.AttitudeIndicator,
        double
    > InternalHeightProperty = AvaloniaProperty.RegisterDirect<
        OldAttitudeIndicator.AttitudeIndicator,
        double
    >(nameof(InternalHeight), _ => _.InternalHeight, (_, value) => _.InternalHeight = value);

    public double InternalHeight
    {
        get;
        set => SetAndRaise(InternalHeightProperty, ref field, value);
    } = 1000;

    public static readonly DirectProperty<
        OldAttitudeIndicator.AttitudeIndicator,
        double
    > PitchTranslateXProperty = AvaloniaProperty.RegisterDirect<
        OldAttitudeIndicator.AttitudeIndicator,
        double
    >(nameof(PitchTranslateX), _ => _.PitchTranslateX, (_, value) => _.PitchTranslateX = value);

    private double PitchTranslateX
    {
        get;
        set => SetAndRaise(PitchTranslateXProperty, ref field, value);
    }

    public static readonly DirectProperty<
        OldAttitudeIndicator.AttitudeIndicator,
        double
    > PitchTranslateYProperty = AvaloniaProperty.RegisterDirect<
        OldAttitudeIndicator.AttitudeIndicator,
        double
    >(nameof(PitchTranslateY), _ => _.PitchTranslateY, (_, value) => _.PitchTranslateY = value);

    public double PitchTranslateY
    {
        get;
        set => SetAndRaise(PitchTranslateYProperty, ref field, value);
    }

    public static readonly DirectProperty<
        OldAttitudeIndicator.AttitudeIndicator,
        IEnumerable<OldAttitudeIndicator.RollItem>
    > RollItemsProperty = AvaloniaProperty.RegisterDirect<
        OldAttitudeIndicator.AttitudeIndicator,
        IEnumerable<OldAttitudeIndicator.RollItem>
    >(nameof(RollItems), _ => _.RollItems, (_, value) => _.RollItems = value);

    public IEnumerable<OldAttitudeIndicator.RollItem> RollItems
    {
        get;
        set => SetAndRaise(RollItemsProperty, ref field, value);
    }

    public static readonly DirectProperty<
        OldAttitudeIndicator.AttitudeIndicator,
        IEnumerable<OldAttitudeIndicator.PitchItem>
    > PitchItemsProperty = AvaloniaProperty.RegisterDirect<
        OldAttitudeIndicator.AttitudeIndicator,
        IEnumerable<OldAttitudeIndicator.PitchItem>
    >(nameof(PitchItems), _ => _.PitchItems, (_, value) => _.PitchItems = value);

    public IEnumerable<OldAttitudeIndicator.PitchItem> PitchItems
    {
        get;
        set => SetAndRaise(PitchItemsProperty, ref field, value);
    }

    public static readonly DirectProperty<
        OldAttitudeIndicator.AttitudeIndicator,
        IEnumerable<OldAttitudeIndicator.ScaleItem>
    > VelocityItemsProperty = AvaloniaProperty.RegisterDirect<
        OldAttitudeIndicator.AttitudeIndicator,
        IEnumerable<OldAttitudeIndicator.ScaleItem>
    >(nameof(VelocityItems), _ => _.VelocityItems, (_, value) => _.VelocityItems = value);

    public IEnumerable<OldAttitudeIndicator.ScaleItem> VelocityItems
    {
        get;
        set => SetAndRaise(VelocityItemsProperty, ref field, value);
    }

    public static readonly DirectProperty<
        OldAttitudeIndicator.AttitudeIndicator,
        IEnumerable<OldAttitudeIndicator.ScaleItem>
    > AltitudeItemsProperty = AvaloniaProperty.RegisterDirect<
        OldAttitudeIndicator.AttitudeIndicator,
        IEnumerable<OldAttitudeIndicator.ScaleItem>
    >(nameof(AltitudeItems), _ => _.AltitudeItems, (_, value) => _.AltitudeItems = value);

    public IEnumerable<OldAttitudeIndicator.ScaleItem> AltitudeItems
    {
        get;
        set => SetAndRaise(AltitudeItemsProperty, ref field, value);
    }

    public static readonly DirectProperty<
        OldAttitudeIndicator.AttitudeIndicator,
        IEnumerable<OldAttitudeIndicator.ScaleItem>
    > HeadingItemsProperty = AvaloniaProperty.RegisterDirect<
        OldAttitudeIndicator.AttitudeIndicator,
        IEnumerable<OldAttitudeIndicator.ScaleItem>
    >(nameof(HeadingItems), _ => _.HeadingItems, (_, value) => _.HeadingItems = value);

    public IEnumerable<OldAttitudeIndicator.ScaleItem> HeadingItems
    {
        get;
        set => SetAndRaise(HeadingItemsProperty, ref field, value);
    }

    public static readonly DirectProperty<
        OldAttitudeIndicator.AttitudeIndicator,
        double
    > HomeAzimuthPositionProperty = AvaloniaProperty.RegisterDirect<
        OldAttitudeIndicator.AttitudeIndicator,
        double
    >(
        nameof(HomeAzimuthPosition),
        _ => _.HomeAzimuthPosition,
        (_, value) => _.HomeAzimuthPosition = value
    );

    public double HomeAzimuthPosition
    {
        get;
        set => SetAndRaise(HomeAzimuthPositionProperty, ref field, value);
    } = -100;

    #endregion
}
