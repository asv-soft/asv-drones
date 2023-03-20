using System.Reactive.Disposables;
using System.Reactive.Linq;
using Asv.Avalonia.Map;
using Asv.Common;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using ReactiveUI;

namespace Asv.Drones.Gui.Uav;

public class MissionStatusIndicator : TemplatedControl
{
    #region Consts
    private const double RoundDegrees = 360;
    #endregion

    #region Direct props
        #region Internal width
        private double _internalWidth = 1000;

        private static readonly DirectProperty<MissionStatusIndicator, double> InternalWidthProperty = AvaloniaProperty.RegisterDirect<MissionStatusIndicator, double>(
            nameof(InternalWidth), o => o.InternalWidth, (o, v) => o.InternalWidth = v);

        private double InternalWidth
        {
            get => _internalWidth;
            set => SetAndRaise(InternalWidthProperty, ref _internalWidth, value);
        }
        #endregion

        #region Internal height
        private double _internalHeight = 200;

        private static readonly DirectProperty<MissionStatusIndicator, double> InternalHeightProperty = AvaloniaProperty.RegisterDirect<MissionStatusIndicator, double>(
            nameof(InternalHeight), o => o.InternalHeight, (o, v) => o.InternalHeight = v);

        private double InternalHeight
        {
            get => _internalHeight;
            set => SetAndRaise(InternalHeightProperty, ref _internalHeight, value);
        }
        #endregion

        #region Internal round height
        private double _internalRoundHeight = 1000;

        private static readonly DirectProperty<MissionStatusIndicator, double> InternalRoundHeightProperty = AvaloniaProperty.RegisterDirect<MissionStatusIndicator, double>(
            nameof(InternalRoundHeight), o => o.InternalRoundHeight, (o, v) => o.InternalRoundHeight = v);

        private double InternalRoundHeight
        {
            get => _internalRoundHeight;
            set => SetAndRaise(InternalRoundHeightProperty, ref _internalRoundHeight, value);
        }
        #endregion
        
        #region Current angle
        private double _currentAngle;

        private static readonly DirectProperty<MissionStatusIndicator, double> CurrentAngleProperty = AvaloniaProperty.RegisterDirect<MissionStatusIndicator, double>(
            nameof(CurrentAngle), o => o.CurrentAngle, (o, v) => o.CurrentAngle = v);

        private double CurrentAngle
        {
            get => _currentAngle;
            set => SetAndRaise(CurrentAngleProperty, ref _currentAngle, value);
        }
        #endregion
        
        #region Completed
        private double _completed = 0;

        private static readonly DirectProperty<MissionStatusIndicator, double> CompletedProperty = AvaloniaProperty.RegisterDirect<MissionStatusIndicator, double>(
            nameof(Completed), o => o.Completed, (o, v) => o.Completed = v);

        private double Completed
        {
            get => _completed;
            set => SetAndRaise(CompletedProperty, ref _completed, value);
        }
        #endregion

        #region Next
        private double _next = 0;

        private static readonly DirectProperty<MissionStatusIndicator, double> NextProperty = AvaloniaProperty.RegisterDirect<MissionStatusIndicator, double>(
            nameof(Next), o => o.Next, (o, v) => o.Next = v);

        private double Next
        {
            get => _next;
            set => SetAndRaise(NextProperty, ref _next, value);
        }
        #endregion

        #region Current waypoint title
        private string _currentWayPointTitle;

        private static readonly DirectProperty<MissionStatusIndicator, string> CurrentWayPointTitleProperty = AvaloniaProperty.RegisterDirect<MissionStatusIndicator, string>(
            nameof(CurrentWayPointTitle), o => o.CurrentWayPointTitle, (o, v) => o.CurrentWayPointTitle = v);

        private string CurrentWayPointTitle
        {
            get => _currentWayPointTitle;
            set => SetAndRaise(CurrentWayPointTitleProperty, ref _currentWayPointTitle, value);
        }
        #endregion

        #region Next waypoint title
        private string _nextWayPointTitle;

        private static readonly DirectProperty<MissionStatusIndicator, string> NextWayPointTitleProperty = AvaloniaProperty.RegisterDirect<MissionStatusIndicator, string>(
            nameof(NextWayPointTitle), o => o.NextWayPointTitle, (o, v) => o.NextWayPointTitle = v);

        private string NextWayPointTitle
        {
            get => _nextWayPointTitle;
            set => SetAndRaise(NextWayPointTitleProperty, ref _nextWayPointTitle, value);
        }
        #endregion
        
    #endregion

    #region Styled props
        #region Current distance
        private static readonly StyledProperty<double> CurrentDistanceProperty = AvaloniaProperty.Register<MissionStatusIndicator, double>(
            nameof(CurrentDistance), notifying: UpdateCurrentDistance);

        public double CurrentDistance
        {
            get => GetValue(CurrentDistanceProperty);
            set => SetValue(CurrentDistanceProperty, value);
        }
        #endregion

        #region Max distance
        private static readonly StyledProperty<double> MaxDistanceProperty = AvaloniaProperty.Register<MissionStatusIndicator, double>(
            nameof(MaxDistance), defaultValue: 0, notifying: UpdateMaxDistance);

        public double MaxDistance
        {
            get => GetValue(MaxDistanceProperty);
            set => SetValue(MaxDistanceProperty, value);
        }
        #endregion
        
    #endregion

    #region Attached props
    #region Items
    public static readonly AttachedProperty<IEnumerable<RoundWayPointItem>> ItemsProperty =
        AvaloniaProperty.RegisterAttached<MissionStatusIndicator, IAvaloniaObject, IEnumerable<RoundWayPointItem>>("Items");
    
    public static void SetItems(IAvaloniaObject element, IEnumerable<RoundWayPointItem> value) 
        => element.SetValue(ItemsProperty, value);
    
    public static IEnumerable<RoundWayPointItem> GetItems(IAvaloniaObject element)
        => (IEnumerable<RoundWayPointItem>)element.GetValue(ItemsProperty);
    #endregion
    
    #endregion
    
    public MissionStatusIndicator()
    {
        SetItems(this, new AvaloniaList<RoundWayPointItem>()
        {
            new() { Altitude = 50, Distance = 0, Title = "WP 0"},
            new() { Altitude = 50, Distance = 130, Title = "WP 1"},
            new() { Altitude = 100, Distance = 185, Title = "WP 2"},
            new() { Altitude = 100, Distance = 213, Title = "WP 3"},
            new() { Altitude = 60, Distance = 164, Title = "WP 4"},
            new() { Altitude = 60, Distance = 108, Title = "WP 5"},
            new() { Altitude = 70, Distance = 321, Title = "WP 6"},
            new() { Altitude = 70, Distance = 232, Title = "WP 7"}
        });
        
        //WayPoints = new AvaloniaList<RoundWayPointItem>()
        //{
        //    new() { Altitude = 50, Distance = 0, Title = "WP 0"},
        //    new() { Altitude = 50, Distance = 130, Title = "WP 1"},
        //    new() { Altitude = 100, Distance = 185, Title = "WP 2"},
        //    new() { Altitude = 100, Distance = 213, Title = "WP 3"},
        //    new() { Altitude = 60, Distance = 164, Title = "WP 4"},
        //    new() { Altitude = 60, Distance = 108, Title = "WP 5"},
        //    new() { Altitude = 70, Distance = 321, Title = "WP 6"},
        //    new() { Altitude = 70, Distance = 232, Title = "WP 7"}
        //};
    }
    
    private static void UpdateCurrentDistance(IAvaloniaObject source, bool beforeChanged)
    {
        if (source is not MissionStatusIndicator indicator) return;

        if (indicator.CurrentDistance == 0 | indicator.CurrentDistance >= indicator.MaxDistance) return;
        
        double aggregatedDistance = 0;
        
        var currentDistance = indicator.CurrentDistance;

        indicator.CurrentAngle = -GetAngle(currentDistance, indicator.MaxDistance);

        indicator.Completed = -indicator.CurrentAngle / RoundDegrees;        
        
        foreach (var item in GetItems(indicator))
        {
            aggregatedDistance += item.Distance;

            if (aggregatedDistance <= currentDistance)
            {
                item.Passed = true;
            }
        }

        double nextDistance = GetItems(indicator).Where(_ => !_.Passed)?.FirstOrDefault()?.Distance ?? (indicator.MaxDistance - aggregatedDistance);

        indicator.Next = GetItems(indicator).Where(_ => _.Passed).Sum(_ => _.Distance) - currentDistance + nextDistance;
        
        indicator.CurrentWayPointTitle = GetItems(indicator).LastOrDefault(_ => _.Passed)?.Title ?? "Home";

        indicator.NextWayPointTitle = GetItems(indicator).FirstOrDefault(_ => !_.Passed)?.Title ?? "Home";
    }

    private static void UpdateMaxDistance(IAvaloniaObject source, bool beforeChanged)
    {
        if (source is not MissionStatusIndicator indicator) return;

        if (GetItems(indicator) is null) return;
        
        double aggregatedDistance = 0;
        
        foreach (var item in GetItems(indicator))
        {
            aggregatedDistance += item.Distance;
            
            item.Angle = GetAngle(aggregatedDistance, indicator.MaxDistance);
        }
        
        UpdateCurrentDistance(source, beforeChanged);
    }
    
    private static double GetAngle(double value, double max) => value * RoundDegrees / max;
}

public class RoundWayPointItem : AvaloniaObject
{
    #region Distance
    private double _distance;

    private static readonly DirectProperty<RoundWayPointItem, double> DistanceProperty = AvaloniaProperty.RegisterDirect<RoundWayPointItem, double>(
        nameof(Distance), o => o.Distance, (o, v) => o.Distance = v);

    /// <summary>
    /// Distance aggregated from whole path before to that waypoint
    /// </summary>
    public double Distance
    {
        get => _distance;
        set => SetAndRaise(DistanceProperty, ref _distance, value);
    }
    #endregion

    #region Altitude
    private double _altitude;

    private static readonly DirectProperty<RoundWayPointItem, double> AltitudeProperty = AvaloniaProperty.RegisterDirect<RoundWayPointItem, double>(
        nameof(Altitude), o => o.Altitude, (o, v) => o.Altitude = v);

    /// <summary>
    /// Altitude above ground level (AGL)
    /// </summary>
    public double Altitude
    {
        get => _altitude;
        set => SetAndRaise(AltitudeProperty, ref _altitude, value);
    }
    #endregion

    #region Angle
    private double _angle;

    private static readonly DirectProperty<RoundWayPointItem, double> AngleProperty = AvaloniaProperty.RegisterDirect<RoundWayPointItem, double>(
        "propertyName", o => o.Angle, (o, v) => o.Angle = v);

    /// <summary>
    /// Angle on a round custom progress bar, depends on distance 
    /// </summary>
    public double Angle
    {
        get => _angle;
        set => SetAndRaise(AngleProperty, ref _angle, value);
    }
    #endregion

    #region Title
    private string _title = "";

    private static readonly DirectProperty<RoundWayPointItem, string> TitleProperty = AvaloniaProperty.RegisterDirect<RoundWayPointItem, string>(
        nameof(Title), o => o.Title, (o, v) => o.Title = v);

    /// <summary>
    /// Waypoint title
    /// </summary>
    public string Title
    {
        get => _title;
        set => SetAndRaise(TitleProperty, ref _title, value);
    }
    #endregion

    #region Passed
    private bool _passed;

    public static readonly DirectProperty<RoundWayPointItem, bool> PassedProperty = AvaloniaProperty.RegisterDirect<RoundWayPointItem, bool>(
        nameof(Passed), o => o.Passed, (o, v) => o.Passed = v);

    public bool Passed
    {
        get => _passed;
        set => SetAndRaise(PassedProperty, ref _passed, value);
    }
    #endregion
}