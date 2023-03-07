using System.Reactive.Linq;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using ReactiveUI;

namespace Asv.Drones.Gui.Uav
{
    public class AttitudeIndicator : TemplatedControl
    {
        private const int VelocityItemCount = 6;
        private const int VelocityValueRange = 5;
        private const double VelocityControlLengthPrc = 0.4;
        private const int AltitudeItemCount = 6;
        private const int AltitudeValueRange = 5;
        private const double AltitudeControlLengthPrc = 0.4;
        private const int HeadingItemCount = 10;
        private const double HeadingControlLengthPrc = 1.0;
        private const int HeadingValueRange = 15;

        private static double _headingPositionStep;
        private static double _headingCenterPosition;

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

        public double Scale { get; }

        public static readonly StyledProperty<double> RollAngleProperty = AvaloniaProperty.Register<AttitudeIndicator, double>(nameof(RollAngle), default(double), notifying: UpdateRollAngle);

        public double RollAngle
        {
            get => GetValue(RollAngleProperty);
            set => SetValue(RollAngleProperty, value);
        }

        public static readonly StyledProperty<double> PitchAngleProperty = AvaloniaProperty.Register<AttitudeIndicator, double>(nameof(PitchAngle), default(double), notifying: UpdateAngle);

        public double PitchAngle
        {
            get => GetValue(PitchAngleProperty);
            set => SetValue(PitchAngleProperty, value);
        }

        public static readonly StyledProperty<double> VelocityProperty =
            AvaloniaProperty.Register<AttitudeIndicator, double>(nameof(Velocity), default(double),
                notifying: UpdateVelocityItems);

        public double Velocity
        {
            get => GetValue(VelocityProperty);
            set => SetValue(VelocityProperty, value);
        }

        public static readonly StyledProperty<double> AltitudeProperty = AvaloniaProperty.Register<AttitudeIndicator, double>(nameof(Altitude), default(double), notifying: UpdateAltitudeItems);

        public double Altitude
        {
            get => GetValue(AltitudeProperty);
            set => SetValue(AltitudeProperty, value);
        }

        public static readonly StyledProperty<double> HeadingProperty = AvaloniaProperty.Register<AttitudeIndicator, double>(nameof(Heading), default(double), notifying: UpdateHeadingItems);

        public double Heading
        {
            get => GetValue(HeadingProperty);
            set => SetValue(HeadingProperty, value);
        }

        public static readonly StyledProperty<double?> HomeAzimuthProperty = AvaloniaProperty.Register<AttitudeIndicator, double?>(nameof(HomeAzimuth), default(double?), notifying: UpdateHomeAzimuthPosition);

        public double? HomeAzimuth
        {
            get => GetValue(HomeAzimuthProperty);
            set => SetValue(HomeAzimuthProperty, value);
        }

        public static readonly StyledProperty<bool> IsArmedProperty = AvaloniaProperty.Register<AttitudeIndicator, bool>(nameof(IsArmed), default(bool));

        public bool IsArmed
        {
            get => GetValue(IsArmedProperty);
            set => SetValue(IsArmedProperty, value);
        }

        public static readonly DirectProperty<AttitudeIndicator, string> StatusTextProperty =
            AvaloniaProperty.RegisterDirect<AttitudeIndicator, string>(nameof(StatusText), _ => _.StatusText,
                (_, value) => _.StatusText = value);

        public string StatusText
        {
            get => _statusText;
            set => SetAndRaise(StatusTextProperty, ref _statusText, value);
        }

        public static readonly DirectProperty<AttitudeIndicator, string> RightStatusTextProperty =
            AvaloniaProperty.RegisterDirect<AttitudeIndicator, string>(nameof(RightStatusText), _ => _.RightStatusText,
                (_, value) => _.RightStatusText = value);

        public string RightStatusText
        {
            get => _rightStatusText;
            set => SetAndRaise(RightStatusTextProperty, ref _rightStatusText, value);
        }

        public static readonly StyledProperty<TimeSpan> ArmedTimeProperty = AvaloniaProperty.Register<AttitudeIndicator, TimeSpan>(nameof(ArmedTime), default(TimeSpan));
        
        public TimeSpan ArmedTime
        {
            get => GetValue(ArmedTimeProperty);
            set => SetValue(ArmedTimeProperty, value);
        }

        #region Internal direct property

        private static readonly DirectProperty<AttitudeIndicator, double> InternalWidthProperty =
            AvaloniaProperty.RegisterDirect<AttitudeIndicator, double>(nameof(InternalWidth), _ => _.InternalWidth,
                (_, value) => _.InternalWidth = value);

        private double InternalWidth
        {
            get => _internalWidth;
            set => SetAndRaise(InternalWidthProperty, ref _internalWidth, value);
        }

        private static readonly DirectProperty<AttitudeIndicator, double> InternalHeightProperty =
            AvaloniaProperty.RegisterDirect<AttitudeIndicator, double>(nameof(InternalHeight), _ => _.InternalHeight,
                (_, value) => _.InternalHeight = value);

        private double InternalHeight
        {
            get => _internalHeight;
            set => SetAndRaise(InternalHeightProperty, ref _internalHeight, value);
        }

        private static readonly DirectProperty<AttitudeIndicator, double> PitchTranslateXProperty =
            AvaloniaProperty.RegisterDirect<AttitudeIndicator, double>(nameof(PitchTranslateX), _ => _.PitchTranslateX,
                (_, value) => _.PitchTranslateX = value);

        private double PitchTranslateX
        {
            get => _pitchTranslateX;
            set => SetAndRaise(PitchTranslateXProperty, ref _pitchTranslateX, value);
        }

        private static readonly DirectProperty<AttitudeIndicator, double> PitchTranslateYProperty =
            AvaloniaProperty.RegisterDirect<AttitudeIndicator, double>(nameof(PitchTranslateY), _ => _.PitchTranslateY,
                (_, value) => _.PitchTranslateY = value);

        private double PitchTranslateY
        {
            get => _pitchTranslateY;
            set => SetAndRaise(PitchTranslateYProperty, ref _pitchTranslateY, value);
        }

        private static readonly DirectProperty<AttitudeIndicator, IEnumerable<RollItem>> RollItemsProperty =
            AvaloniaProperty.RegisterDirect<AttitudeIndicator, IEnumerable<RollItem>>(nameof(RollItems),
                _ => _.RollItems, (_, value) => _.RollItems = value);

        private IEnumerable<RollItem> RollItems
        {
            get => _rollItems;
            set => SetAndRaise(RollItemsProperty, ref _rollItems, value);
        }

        private static readonly DirectProperty<AttitudeIndicator, IEnumerable<PitchItem>> PitchItemsProperty =
            AvaloniaProperty.RegisterDirect<AttitudeIndicator, IEnumerable<PitchItem>>(nameof(PitchItems),
                _ => _.PitchItems, (_, value) => _.PitchItems = value);

        private IEnumerable<PitchItem> PitchItems
        {
            get => _pitchItems;
            set => SetAndRaise(PitchItemsProperty, ref _pitchItems, value);
        }

        private static readonly DirectProperty<AttitudeIndicator, IEnumerable<ScaleItem>> VelocityItemsProperty =
            AvaloniaProperty.RegisterDirect<AttitudeIndicator, IEnumerable<ScaleItem>>(nameof(VelocityItems),
                _ => _.VelocityItems, (_, value) => _.VelocityItems = value);

        private IEnumerable<ScaleItem> VelocityItems
        {
            get => _velocityItems;
            set => SetAndRaise(VelocityItemsProperty, ref _velocityItems, value);
        }

        private static readonly DirectProperty<AttitudeIndicator, IEnumerable<ScaleItem>> AltitudeItemsProperty =
            AvaloniaProperty.RegisterDirect<AttitudeIndicator, IEnumerable<ScaleItem>>(nameof(AltitudeItems),
                _ => _.AltitudeItems, (_, value) => _.AltitudeItems = value);

        private IEnumerable<ScaleItem> AltitudeItems
        {
            get => _altitudeItems;
            set => SetAndRaise(AltitudeItemsProperty, ref _altitudeItems, value);
        }

        private static readonly DirectProperty<AttitudeIndicator, IEnumerable<ScaleItem>> HeadingItemsProperty =
            AvaloniaProperty.RegisterDirect<AttitudeIndicator, IEnumerable<ScaleItem>>(nameof(HeadingItems),
                _ => _.HeadingItems, (_, value) => _.HeadingItems = value);

        private IEnumerable<ScaleItem> HeadingItems
        {
            get => _headingItems;
            set => SetAndRaise(HeadingItemsProperty, ref _headingItems, value);
        }

        private static readonly DirectProperty<AttitudeIndicator, double> HomeAzimuthPositionProperty =
            AvaloniaProperty.RegisterDirect<AttitudeIndicator, double>(nameof(HomeAzimuthPosition),
                _ => _.HomeAzimuthPosition, (_, value) => _.HomeAzimuthPosition = value);

        
        private double HomeAzimuthPosition
        {
            get => _homeAzimuthPosition;
            set => SetAndRaise(HomeAzimuthPositionProperty, ref _homeAzimuthPosition, value);
        }

        #endregion


        public AttitudeIndicator()
        {
            if (Design.IsDesignMode)
            {
                
                var status = new[] { "Armed", "Disarmed" };
                Observable.Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1)).ObserveOn(RxApp.MainThreadScheduler).Subscribe(_ =>
                {
                    StatusText = status[_ % 2];
                });
                StatusText = status[1];
            }

            var smallerSide = Math.Min(InternalWidth, InternalHeight);
            Scale = smallerSide / 100;

            RollItems = new AvaloniaList<RollItem>(
                new RollItem(0), new RollItem(10), new RollItem(20), new RollItem(30), new RollItem(45),
                new RollItem(60), new RollItem(300), new RollItem(315), new RollItem(330), new RollItem(340),
                new RollItem(350));
            
            PitchItems = new AvaloniaList<PitchItem>(
                new PitchItem(135, Scale, false), new PitchItem(130, Scale), new PitchItem(125, Scale, false), new PitchItem(120, Scale),
                new PitchItem(115, Scale, false), new PitchItem(110, Scale), new PitchItem(105, Scale, false), new PitchItem(100, Scale),
                new PitchItem(95, Scale, false), new PitchItem(90, Scale), new PitchItem(85, Scale, false), new PitchItem(80, Scale),
                new PitchItem(75, Scale, false), new PitchItem(70, Scale), new PitchItem(65, Scale, false), new PitchItem(60, Scale),
                new PitchItem(55, Scale, false), new PitchItem(50, Scale), new PitchItem(45, Scale, false), new PitchItem(40, Scale),
                new PitchItem(35, Scale, false), new PitchItem(30, Scale), new PitchItem(25, Scale, false), new PitchItem(20, Scale),
                new PitchItem(15, Scale, false), new PitchItem(10, Scale), new PitchItem(5, Scale, false), new PitchItem(0, Scale),
                new PitchItem(-5, Scale, false), new PitchItem(-10, Scale), new PitchItem(-15, Scale, false), new PitchItem(-20, Scale),
                new PitchItem(-25, Scale, false), new PitchItem(-30, Scale), new PitchItem(-35, Scale, false), new PitchItem(-40, Scale),
                new PitchItem(-45, Scale, false), new PitchItem(-50, Scale), new PitchItem(-55, Scale, false), new PitchItem(-60, Scale),
                new PitchItem(-65, Scale, false), new PitchItem(-70, Scale), new PitchItem(-75, Scale, false), new PitchItem(-80, Scale),
                new PitchItem(-85, Scale, false), new PitchItem(-90, Scale), new PitchItem(-95, Scale, false), new PitchItem(-100, Scale),
                new PitchItem(-105, Scale, false), new PitchItem(-110, Scale), new PitchItem(-115, Scale, false), new PitchItem(-120, Scale),
                new PitchItem(-125, Scale, false), new PitchItem(-130, Scale), new PitchItem(-135, Scale, false)
            );

            var velocityControlLength = smallerSide * VelocityControlLengthPrc;
            var velocityItemLength = velocityControlLength / (VelocityItemCount - 1);
            VelocityItems = new AvaloniaList<ScaleItem>(Enumerable.Range(0, VelocityItemCount).Select(_ =>
                new ScaleItem(0, VelocityValueRange, _, VelocityItemCount, velocityControlLength + velocityItemLength,
                    velocityControlLength, showNegative: false)));
            
            var altitudeControlLength = smallerSide * AltitudeControlLengthPrc;
            var altitudeItemLength = altitudeControlLength / (AltitudeItemCount - 1);
            AltitudeItems = new AvaloniaList<ScaleItem>(Enumerable.Range(0, AltitudeItemCount).Select(_ =>
                new ScaleItem(0, AltitudeValueRange, _, AltitudeItemCount, altitudeControlLength + altitudeItemLength,
                    altitudeControlLength)));

            var headingControlLength = smallerSide * HeadingControlLengthPrc;
            var headingItemLength = headingControlLength / (HeadingItemCount - 1);
            HeadingItems = new AvaloniaList<ScaleItem>(Enumerable.Range(0, HeadingItemCount).Select(_ =>
                new HeadingScaleItem(0, HeadingValueRange, _, HeadingItemCount,
                    headingControlLength + headingItemLength, headingControlLength)));

            var headingItemStep = (headingControlLength + headingItemLength) / (HeadingItemCount % 2 != 0 ? HeadingItemCount - 1 : HeadingItemCount);
            _headingPositionStep = -1 * headingItemStep / HeadingValueRange;
            _headingCenterPosition = headingControlLength / 2;
        }

        private static void UpdateAngle(IAvaloniaObject source, bool beforeChanged)
        {
            if (source is not AttitudeIndicator indicator) return;
            var pitch = indicator.PitchAngle;
            UpdateRollAngle(source, beforeChanged);
            foreach (var item in indicator.PitchItems)
            {
                item.UpdateVisibility(pitch);
            }
        }

        private static void UpdateRollAngle(IAvaloniaObject source, bool beforeChanged)
        {
            if (source is not AttitudeIndicator indicator) return;
            var roll = indicator.RollAngle;
            var pitch = indicator.PitchAngle;
            indicator.PitchTranslateX = -pitch * indicator.Scale * Math.Cos((roll - 90.0) * Math.PI / 180.0);
            indicator.PitchTranslateY = pitch * indicator.Scale * Math.Sin((90 - roll) * Math.PI / 180.0);
        }

        private static void UpdateVelocityItems(IAvaloniaObject source, bool beforeChanged)
        {
            if (source is not AttitudeIndicator indicator) return;
            var velocity = indicator.Velocity;
            foreach (var item in indicator.VelocityItems)
            {
                item.UpdateValue(velocity);
            }
        }

        private static void UpdateAltitudeItems(IAvaloniaObject source, bool beforeChanged)
        {
            if (source is not AttitudeIndicator indicator) return;
            var altitude = indicator.Altitude;
            foreach (var item in indicator.AltitudeItems)
            {
                item.UpdateValue(altitude);
            }
        }

        private static void UpdateHeadingItems(IAvaloniaObject source, bool beforeChanged)
        {
            if (source is not AttitudeIndicator indicator) return;
            var heading = indicator.Heading;
            foreach (var item in indicator.HeadingItems)
            {
                item.UpdateValue(heading);
            }
            indicator.HomeAzimuthPosition = GetHomeAzimuthPosition(indicator.HomeAzimuth, indicator.Heading);
        }

        private static void UpdateHomeAzimuthPosition(IAvaloniaObject source, bool beforeChanged)
        {
            if (source is not AttitudeIndicator indicator) return;
            indicator.HomeAzimuthPosition = GetHomeAzimuthPosition(indicator.HomeAzimuth, indicator.Heading);
        }

        private static double GetHomeAzimuthPosition(double? value, double headingValue)
        {
            if (value == null) return -100;

            var distance = (headingValue - value.Value) % 360;
            if (distance < -180)
                distance += 360;
            else if (distance > 179)
                distance -= 360;

            return _headingCenterPosition + distance * _headingPositionStep;
        }
    }


    public class ScaleItem : AvaloniaObject
    {
        private readonly double _valueRange;
        private readonly bool _showNegative;
        private readonly double _startPosition;
        private readonly double _positionStep;
        private readonly double _valueOffset;
        private readonly bool _isFixedTitle;
        private string _title;
        private double _value;
        private double _position;
        private bool _isVisible;

        public ScaleItem(double value, double valueRange, int index, int itemCount, double fullLength, double length, bool isInverse = false, bool showNegative = true, string fixedTitle = null)
        {
            _valueRange = valueRange;
            _showNegative = showNegative;
            _isFixedTitle = fixedTitle != null;
            var step = fullLength / (itemCount % 2 != 0 ? itemCount - 1 : itemCount);
            _positionStep = step / valueRange;
            
            if (!isInverse)
            {
                _startPosition = (length - fullLength) / 2.0 + step * index;
            }
            else
            {
                _startPosition = (length - fullLength) / 2.0 + step * (itemCount - index);
                _positionStep *= -1;
            }

            var centerIndex = itemCount % 2 == 0 ? itemCount / 2 : itemCount / 2 + 1;
            
            var indexOffset = index - centerIndex;
            _valueOffset = -1 * valueRange * indexOffset;

            if (_isFixedTitle) Title = fixedTitle;
            UpdateValue(value);
        }

        public void UpdateValue(double value)
        {
            Value = GetValue(value);
            Position = GetPosition(value);
            if (!_isFixedTitle)
                Title = GetTitle(Value);
            IsVisible = _showNegative || Value >= 0;
        }

        protected virtual string GetTitle(double value)
        {
             return Math.Round(value).ToString("F0");
        }

        private double GetValue(double value)
        {
            return Math.Round(value) - Math.Round(value) % _valueRange + _valueOffset;
        }

        private double GetPosition(double value)
        {
            return _startPosition + _positionStep * (Math.Round(value) % _valueRange);
        }

        public static readonly DirectProperty<ScaleItem, bool> IsVisibleProperty =
            AvaloniaProperty.RegisterDirect<ScaleItem, bool>(nameof(IsVisible), _ => _.IsVisible,
                (_, value) => _.IsVisible = value);

        public bool IsVisible
        {
            get => _isVisible;
            set => SetAndRaise(IsVisibleProperty, ref _isVisible, value);
        }

        public static readonly DirectProperty<ScaleItem, string> TitleProperty =
            AvaloniaProperty.RegisterDirect<ScaleItem, string>(nameof(Title), _ => _.Title,
                (_, value) => _.Title = value);

        public string Title
        {
            get => _title;
            set => SetAndRaise(TitleProperty, ref _title, value);
        }

        public static readonly DirectProperty<ScaleItem, double> ValueProperty =
            AvaloniaProperty.RegisterDirect<ScaleItem, double>(nameof(Value), _ => _.Value,
                (_, value) => _.Value = value);

        public double Value
        {
            get => _value;
            set => SetAndRaise(ValueProperty, ref _value, value);
        }

        public static readonly DirectProperty<ScaleItem, double> PositionProperty =
            AvaloniaProperty.RegisterDirect<ScaleItem, double>(nameof(Position), _ => _.Position,
                (_, value) => _.Position = value);

        public double Position
        {
            get => _position;
            set => SetAndRaise(PositionProperty, ref _position, value);
        }
    }

    public class HeadingScaleItem : ScaleItem
    {
        public HeadingScaleItem(double value, double valueRange, int index, int itemCount, double fullLength, double length) : base(value, valueRange, index, itemCount, fullLength, length, true)
        {

        }

        protected override string GetTitle(double value)
        {
            var v = value < 0 ? ((int)Math.Round(value) % 360) + 360 : (int)Math.Round(value) % 360;
            return v switch
            {
                0 => "N",
                45 => "NE",
                90 => "E",
                135 => "SE",
                180 => "S",
                225 => "SW",
                270 => "W",
                315 => "NW",
                360 => "N",
                _ => v.ToString("F0")
            };
        }
    }

    public class RollItem : AvaloniaObject
    {
        public RollItem(int angle)
        {
            Value = angle;
            Title = Math.Abs(angle) > 180 ? (360 - Math.Abs(angle)).ToString() : Math.Abs(angle).ToString();
        }

        public static readonly DirectProperty<RollItem, string> TitleProperty =
            AvaloniaProperty.RegisterDirect<RollItem, string>(nameof(Title), _ => _.Title,
                (_, value) => _.Title = value);

        private string _title;
        private double _value;

        public string Title
        {
            get => _title;
            set => SetAndRaise(TitleProperty, ref _title, value);
        }

        public static readonly DirectProperty<RollItem, double> ValueProperty =
            AvaloniaProperty.RegisterDirect<RollItem, double>(nameof(Value), _ => _.Value,
                (_, value) => _.Value = value);

        public double Value
        {
            get => _value;
            set => SetAndRaise(ValueProperty, ref _value, value);
        }
    }

    public class PitchItem : AvaloniaObject
    {
        private readonly int _pitch;
        private string _title;
        private double _value;
        private Point _startLine;
        private Point _stopLine;
        private bool _isVisible;

        public PitchItem(int pitch, double scale, bool titleIsVisible = true, double controlHeight = 284)
        {
            _pitch = pitch;
            Value = (controlHeight / 2 - pitch) * scale;
            if (titleIsVisible)
            {
                Title = pitch.ToString();
                StartLine = new Point(0 * scale, 0 * scale);
                StopLine = new Point(20 * scale, 0 * scale);
            }
            else
            {
                Title = null;
                StartLine = new Point(4 * scale, 0 * scale);
                StopLine = new Point(16 * scale, 0 * scale);
            }

            IsVisible = Math.Abs(pitch) <= 20;
        }

        public static readonly DirectProperty<PitchItem, string> TitleProperty =
            AvaloniaProperty.RegisterDirect<PitchItem, string>(nameof(Title), _ => _.Title,
                (_, value) => _.Title = value);

        public string Title
        {
            get => _title;
            set => SetAndRaise(TitleProperty, ref _title, value);
        }

        public static readonly DirectProperty<PitchItem, double> ValueProperty =
            AvaloniaProperty.RegisterDirect<PitchItem, double>(nameof(Value), _ => _.Value,
                (_, value) => _.Value = value);
        
        public double Value
        {
            get => _value;
            set => SetAndRaise(ValueProperty, ref _value, value);
        }

        public static readonly DirectProperty<PitchItem, bool> IsVisibleProperty =
            AvaloniaProperty.RegisterDirect<PitchItem, bool>(nameof(IsVisible), _ => _.IsVisible,
                (_, value) => _.IsVisible = value);

        
        public bool IsVisible
        {
            get => _isVisible;
            set => SetAndRaise(IsVisibleProperty, ref _isVisible, value);
        }

        public static readonly DirectProperty<PitchItem, Point> StartLineProperty =
            AvaloniaProperty.RegisterDirect<PitchItem, Point>(nameof(StartLine), _ => _.StartLine,
                (_, value) => _.StartLine = value);

        public Point StartLine
        {
            get => _startLine;
            set => SetAndRaise(StartLineProperty, ref _startLine, value);
        }

        public static readonly DirectProperty<PitchItem, Point> StopLineProperty =
            AvaloniaProperty.RegisterDirect<PitchItem, Point>(nameof(StopLine), _ => _.StopLine,
                (_, value) => _.StopLine = value);

        public Point StopLine
        {
            get => _stopLine;
            set => SetAndRaise(StopLineProperty, ref _stopLine, value);
        }

        public void UpdateVisibility(double pitch)
        {
            IsVisible = pitch >= _pitch - 20 && pitch <= _pitch + 20;
        }
    }
}