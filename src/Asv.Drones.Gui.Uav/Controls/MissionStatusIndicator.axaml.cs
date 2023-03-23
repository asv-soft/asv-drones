using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Mavlink;
using Asv.Mavlink.Vehicle;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using DynamicData;
using DynamicData.Binding;
using FluentAvalonia.Core;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Disposable = System.Reactive.Disposables.Disposable;
using Location = Asv.Avalonia.Map.Location;

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
        public static readonly StyledProperty<double> CurrentDistanceProperty = AvaloniaProperty.Register<MissionStatusIndicator, double>(
            nameof(CurrentDistance), notifying: UpdateCurrentDistance);
        
        public double CurrentDistance
        {
            get => GetValue(CurrentDistanceProperty);
            set => SetValue(CurrentDistanceProperty, value);
        }
        #endregion

        #region Max distance
        private static readonly StyledProperty<double> MaxDistanceProperty = AvaloniaProperty.Register<MissionStatusIndicator, double>(
            nameof(MaxDistance), defaultValue: 0);
        
        private double MaxDistance
        {
            get => GetValue(MaxDistanceProperty);
            set => SetValue(MaxDistanceProperty, value);
        }
        #endregion

        #region WayPoints
        public static readonly StyledProperty<ReadOnlyObservableCollection<RoundWayPointItem>> WayPointsProperty = AvaloniaProperty.Register<MissionStatusIndicator, ReadOnlyObservableCollection<RoundWayPointItem>>(
            nameof(WayPoints));

        private IObservable<IChangeSet<RoundWayPointItem>> _changeSet;

        public ReadOnlyObservableCollection<RoundWayPointItem> WayPoints
        {
            get => GetValue(WayPointsProperty);
            set => SetValue(WayPointsProperty, value);
        }
        #endregion
        
    #endregion

    public MissionStatusIndicator()
    {
        this.WhenValueChanged(_ => _.WayPoints).Subscribe(OnAttachedNewCollection);
    }
    
    private void OnAttachedNewCollection(ReadOnlyObservableCollection<RoundWayPointItem>? collection)
    {
        if(collection is null) return;

        _changeSet = collection.ToObservableChangeSet();
        _changeSet.WhenPropertyChanged(_ => _.Location, false)
            .Throttle(TimeSpan.FromMilliseconds(200))
            .Subscribe(_ => UpdateEverything());
        
        UpdateEverything();
    }


    private void UpdateEverything()
    {
        for (int i = 1; i < WayPoints.Count; i++)
        {
            WayPoints[i].Distance = GeoMath.Distance(
                WayPoints[i - 1].Location,WayPoints[i].Location);
        }

        MaxDistance = WayPoints.Sum(_ => _.Distance);

        UpdateAngles();
    }

    private void UpdateAngles()
    {
        double aggregatedDistance = 0;
        
        for (int i = 1; i < WayPoints.Count; i++)
        {
            aggregatedDistance += WayPoints[i].Distance;
            
            WayPoints[i].Angle = GetAngle(aggregatedDistance, MaxDistance + (MaxDistance * 0.1));
        }
    }
    
    private static void UpdateCurrentDistance(IAvaloniaObject source, bool beforeChanged)
    {
        if (source is not MissionStatusIndicator indicator) return;
        
        if (indicator.CurrentDistance == 0 | indicator.CurrentDistance >= indicator.MaxDistance) return;
        
        double aggregatedDistance = 0;
        
        var currentDistance = indicator.CurrentDistance;

        indicator.CurrentAngle = -GetAngle(currentDistance, indicator.MaxDistance + (indicator.MaxDistance * 0.1));

        indicator.Completed = GetAngle(currentDistance, indicator.MaxDistance) / RoundDegrees;        
        
        foreach (var item in indicator.WayPoints)
        {
            aggregatedDistance += item.Distance;

            if (aggregatedDistance <= currentDistance)
            {
                item.Passed = true;
            }
        }

        double nextDistance = indicator.WayPoints.Where(_ => !_.Passed)?.FirstOrDefault()?.Distance ?? (indicator.MaxDistance - aggregatedDistance);

        indicator.Next = indicator.WayPoints.Where(_ => _.Passed).Sum(_ => _.Distance) - currentDistance + nextDistance;
        
        indicator.CurrentWayPointTitle = indicator.WayPoints.LastOrDefault(_ => _.Passed)?.Title ?? "Home";

        indicator.NextWayPointTitle = indicator.WayPoints.FirstOrDefault(_ => !_.Passed)?.Title ?? "Home";
    }

    private static double GetAngle(double value, double max) => value * RoundDegrees / max;
}

public class RoundWayPointItem : AvaloniaObject
{
    public RoundWayPointItem(MissionItem item)
    {
        item.Location.Subscribe(_ => Location = _);
        Title = $"WP {item.Index}";
    }

    #region Location
    private GeoPoint _location;
    
    private static readonly DirectProperty<RoundWayPointItem, GeoPoint> LocationProperty = AvaloniaProperty.RegisterDirect<RoundWayPointItem, GeoPoint>(
        nameof(Location), o => o.Location, (o, v) => o.Location = v);

    /// <summary>
    /// Distance aggregated from whole path before to that waypoint
    /// </summary>
    public GeoPoint Location
    {
        get => _location;
        set => SetAndRaise(LocationProperty, ref _location, value);
    }
    #endregion

    #region Altitude
    public double Altitude => Location.Altitude;
    #endregion
    
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