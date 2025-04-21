using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;

namespace Asv.Drones;

public partial class AttitudeIndicator
{
    private IEnumerable<PitchItem> _pitchItems;
    private IEnumerable<RollItem> _rollItems;
    private IEnumerable<ScaleItem> _velocityItems;
    private IEnumerable<ScaleItem> _altitudeItems;
    private IEnumerable<ScaleItem> _headingItems;
    private double _internalWidth = 1000;
    private double _internalHeight = 1000;
    private double _pitchTranslateX;
    private double _pitchTranslateY;
    private double _homeAzimuthPosition = -100;
    private string _statusText;
    private string _rightStatusText;
    private Color _brushVibrationX;

    public static readonly DirectProperty<AttitudeIndicator, Color> BrushVibrationXProperty =
        AvaloniaProperty.RegisterDirect<AttitudeIndicator, Color>(
            nameof(BrushVibrationX),
            o => o.BrushVibrationX,
            (o, v) => o.BrushVibrationX = v
        );

    public Color BrushVibrationX
    {
        get => _brushVibrationX;
        set => SetAndRaise(BrushVibrationXProperty, ref _brushVibrationX, value);
    }

    private Color _brushVibrationY;

    public static readonly DirectProperty<AttitudeIndicator, Color> BrushVibrationYProperty =
        AvaloniaProperty.RegisterDirect<AttitudeIndicator, Color>(
            nameof(BrushVibrationY),
            o => o.BrushVibrationY,
            (o, v) => o.BrushVibrationY = v
        );

    public Color BrushVibrationY
    {
        get => _brushVibrationY;
        set => SetAndRaise(BrushVibrationYProperty, ref _brushVibrationY, value);
    }

    private Color _brushVibrationZ;

    public static readonly DirectProperty<AttitudeIndicator, Color> BrushVibrationZProperty =
        AvaloniaProperty.RegisterDirect<AttitudeIndicator, Color>(
            nameof(BrushVibrationZ),
            o => o.BrushVibrationZ,
            (o, v) => o.BrushVibrationZ = v
        );

    public Color BrushVibrationZ
    {
        get => _brushVibrationZ;
        set => SetAndRaise(BrushVibrationZProperty, ref _brushVibrationZ, value);
    }

    public static readonly StyledProperty<float> VibrationXProperty = AvaloniaProperty.Register<
        AttitudeIndicator,
        float
    >(nameof(VibrationX), defaultValue: -1);

    public float VibrationX
    {
        get => GetValue(VibrationXProperty);
        set => SetValue(VibrationXProperty, value);
    }

    public static readonly StyledProperty<float> VibrationYProperty = AvaloniaProperty.Register<
        AttitudeIndicator,
        float
    >(nameof(VibrationY), defaultValue: -1);

    public float VibrationY
    {
        get => GetValue(VibrationYProperty);
        set => SetValue(VibrationYProperty, value);
    }

    public static readonly StyledProperty<float> VibrationZProperty = AvaloniaProperty.Register<
        AttitudeIndicator,
        float
    >(nameof(VibrationZ), defaultValue: -1);

    public float VibrationZ
    {
        get => GetValue(VibrationZProperty);
        set => SetValue(VibrationZProperty, value);
    }

    public static readonly StyledProperty<uint> Clipping0Property = AvaloniaProperty.Register<
        AttitudeIndicator,
        uint
    >(nameof(Clipping0));

    public uint Clipping0
    {
        get => GetValue(Clipping0Property);
        set => SetValue(Clipping0Property, value);
    }

    public static readonly StyledProperty<uint> Clipping1Property = AvaloniaProperty.Register<
        AttitudeIndicator,
        uint
    >(nameof(Clipping1));

    public uint Clipping1
    {
        get => GetValue(Clipping1Property);
        set => SetValue(Clipping1Property, value);
    }

    public static readonly StyledProperty<uint> Clipping2Property = AvaloniaProperty.Register<
        AttitudeIndicator,
        uint
    >(nameof(Clipping2));

    public uint Clipping2
    {
        get => GetValue(Clipping2Property);
        set => SetValue(Clipping2Property, value);
    }

    public static readonly StyledProperty<double> RollAngleProperty = AvaloniaProperty.Register<
        AttitudeIndicator,
        double
    >(nameof(RollAngle));

    public double RollAngle
    {
        get => GetValue(RollAngleProperty);
        set => SetValue(RollAngleProperty, value);
    }

    public static readonly StyledProperty<double> PitchAngleProperty = AvaloniaProperty.Register<
        AttitudeIndicator,
        double
    >(nameof(PitchAngle));

    public double PitchAngle
    {
        get => GetValue(PitchAngleProperty);
        set => SetValue(PitchAngleProperty, value);
    }

    public static readonly StyledProperty<double> VelocityProperty = AvaloniaProperty.Register<
        AttitudeIndicator,
        double
    >(nameof(Velocity));

    public double Velocity
    {
        get => GetValue(VelocityProperty);
        set => SetValue(VelocityProperty, value);
    }

    public static readonly StyledProperty<double> AltitudeProperty = AvaloniaProperty.Register<
        AttitudeIndicator,
        double
    >(nameof(Altitude));

    public double Altitude
    {
        get => GetValue(AltitudeProperty);
        set => SetValue(AltitudeProperty, value);
    }

    public static readonly StyledProperty<double> HeadingProperty = AvaloniaProperty.Register<
        AttitudeIndicator,
        double
    >(nameof(Heading));

    public double Heading
    {
        get => GetValue(HeadingProperty);
        set => SetValue(HeadingProperty, value);
    }

    public static readonly StyledProperty<double?> HomeAzimuthProperty = AvaloniaProperty.Register<
        AttitudeIndicator,
        double?
    >(nameof(HomeAzimuth));

    public double? HomeAzimuth
    {
        get => GetValue(HomeAzimuthProperty);
        set => SetValue(HomeAzimuthProperty, value);
    }

    public static readonly StyledProperty<bool> IsArmedProperty = AvaloniaProperty.Register<
        AttitudeIndicator,
        bool
    >(nameof(IsArmed));

    public bool IsArmed
    {
        get => GetValue(IsArmedProperty);
        set => SetValue(IsArmedProperty, value);
    }

    public static readonly DirectProperty<AttitudeIndicator, string> StatusTextProperty =
        AvaloniaProperty.RegisterDirect<AttitudeIndicator, string>(
            nameof(StatusText),
            _ => _.StatusText,
            (_, value) => _.StatusText = value
        );

    public string StatusText
    {
        get => _statusText;
        set => SetAndRaise(StatusTextProperty, ref _statusText, value);
    }

    public static readonly DirectProperty<AttitudeIndicator, string> RightStatusTextProperty =
        AvaloniaProperty.RegisterDirect<AttitudeIndicator, string>(
            nameof(RightStatusText),
            _ => _.RightStatusText,
            (_, value) => _.RightStatusText = value
        );

    public string RightStatusText
    {
        get => _rightStatusText;
        set => SetAndRaise(RightStatusTextProperty, ref _rightStatusText, value);
    }

    public static readonly StyledProperty<TimeSpan> ArmedTimeProperty = AvaloniaProperty.Register<
        AttitudeIndicator,
        TimeSpan
    >(nameof(ArmedTime));

    public TimeSpan ArmedTime
    {
        get => GetValue(ArmedTimeProperty);
        set => SetValue(ArmedTimeProperty, value);
    }

    #region Internal direct property

    public static readonly DirectProperty<AttitudeIndicator, double> InternalWidthProperty =
        AvaloniaProperty.RegisterDirect<AttitudeIndicator, double>(
            nameof(InternalWidth),
            _ => _.InternalWidth,
            (_, value) => _.InternalWidth = value
        );

    public double InternalWidth
    {
        get => _internalWidth;
        set => SetAndRaise(InternalWidthProperty, ref _internalWidth, value);
    }

    public static readonly DirectProperty<AttitudeIndicator, double> InternalHeightProperty =
        AvaloniaProperty.RegisterDirect<AttitudeIndicator, double>(
            nameof(InternalHeight),
            _ => _.InternalHeight,
            (_, value) => _.InternalHeight = value
        );

    public double InternalHeight
    {
        get => _internalHeight;
        set => SetAndRaise(InternalHeightProperty, ref _internalHeight, value);
    }

    public static readonly DirectProperty<AttitudeIndicator, double> PitchTranslateXProperty =
        AvaloniaProperty.RegisterDirect<AttitudeIndicator, double>(
            nameof(PitchTranslateX),
            _ => _.PitchTranslateX,
            (_, value) => _.PitchTranslateX = value
        );

    private double PitchTranslateX
    {
        get => _pitchTranslateX;
        set => SetAndRaise(PitchTranslateXProperty, ref _pitchTranslateX, value);
    }

    public static readonly DirectProperty<AttitudeIndicator, double> PitchTranslateYProperty =
        AvaloniaProperty.RegisterDirect<AttitudeIndicator, double>(
            nameof(PitchTranslateY),
            _ => _.PitchTranslateY,
            (_, value) => _.PitchTranslateY = value
        );

    public double PitchTranslateY
    {
        get => _pitchTranslateY;
        set => SetAndRaise(PitchTranslateYProperty, ref _pitchTranslateY, value);
    }

    public static readonly DirectProperty<
        AttitudeIndicator,
        IEnumerable<RollItem>
    > RollItemsProperty = AvaloniaProperty.RegisterDirect<AttitudeIndicator, IEnumerable<RollItem>>(
        nameof(RollItems),
        _ => _.RollItems,
        (_, value) => _.RollItems = value
    );

    public IEnumerable<RollItem> RollItems
    {
        get => _rollItems;
        set => SetAndRaise(RollItemsProperty, ref _rollItems, value);
    }

    public static readonly DirectProperty<
        AttitudeIndicator,
        IEnumerable<PitchItem>
    > PitchItemsProperty = AvaloniaProperty.RegisterDirect<
        AttitudeIndicator,
        IEnumerable<PitchItem>
    >(nameof(PitchItems), _ => _.PitchItems, (_, value) => _.PitchItems = value);

    public IEnumerable<PitchItem> PitchItems
    {
        get => _pitchItems;
        set => SetAndRaise(PitchItemsProperty, ref _pitchItems, value);
    }

    public static readonly DirectProperty<
        AttitudeIndicator,
        IEnumerable<ScaleItem>
    > VelocityItemsProperty = AvaloniaProperty.RegisterDirect<
        AttitudeIndicator,
        IEnumerable<ScaleItem>
    >(nameof(VelocityItems), _ => _.VelocityItems, (_, value) => _.VelocityItems = value);

    public IEnumerable<ScaleItem> VelocityItems
    {
        get => _velocityItems;
        set => SetAndRaise(VelocityItemsProperty, ref _velocityItems, value);
    }

    public static readonly DirectProperty<
        AttitudeIndicator,
        IEnumerable<ScaleItem>
    > AltitudeItemsProperty = AvaloniaProperty.RegisterDirect<
        AttitudeIndicator,
        IEnumerable<ScaleItem>
    >(nameof(AltitudeItems), _ => _.AltitudeItems, (_, value) => _.AltitudeItems = value);

    public IEnumerable<ScaleItem> AltitudeItems
    {
        get => _altitudeItems;
        set => SetAndRaise(AltitudeItemsProperty, ref _altitudeItems, value);
    }

    public static readonly DirectProperty<
        AttitudeIndicator,
        IEnumerable<ScaleItem>
    > HeadingItemsProperty = AvaloniaProperty.RegisterDirect<
        AttitudeIndicator,
        IEnumerable<ScaleItem>
    >(nameof(HeadingItems), _ => _.HeadingItems, (_, value) => _.HeadingItems = value);

    public IEnumerable<ScaleItem> HeadingItems
    {
        get => _headingItems;
        set => SetAndRaise(HeadingItemsProperty, ref _headingItems, value);
    }

    public static readonly DirectProperty<AttitudeIndicator, double> HomeAzimuthPositionProperty =
        AvaloniaProperty.RegisterDirect<AttitudeIndicator, double>(
            nameof(HomeAzimuthPosition),
            _ => _.HomeAzimuthPosition,
            (_, value) => _.HomeAzimuthPosition = value
        );

    public double HomeAzimuthPosition
    {
        get => _homeAzimuthPosition;
        set => SetAndRaise(HomeAzimuthPositionProperty, ref _homeAzimuthPosition, value);
    }

    #endregion
}
