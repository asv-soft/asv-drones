using System.Collections.ObjectModel;
using System.Reactive.Subjects;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Uav;

public class MissionStatusViewModel : ViewModelBase
{
    public MissionStatusViewModel() : base(new Uri("designTime://missionstatus"))
    {
        if (Design.IsDesignMode)
        {
            WayPoints = new()
            {
                new() { Altitude = 50, Distance = 0, Title = "WP 0" },
                new() { Altitude = 50, Distance = 130, Title = "WP 1" },
                new() { Altitude = 100, Distance = 185, Title = "WP 2" },
                new() { Altitude = 100, Distance = 213, Title = "WP 3" },
                new() { Altitude = 60, Distance = 164, Title = "WP 4" },
                new() { Altitude = 60, Distance = 108, Title = "WP 5" },
                new() { Altitude = 70, Distance = 321, Title = "WP 6" },
                new() { Altitude = 70, Distance = 232, Title = "WP 7" },
                new() { Altitude = 50, Distance = 120, Title = "WP 8" },
                new() { Altitude = 50, Distance = 130, Title = "WP 9" },
                new() { Altitude = 100, Distance = 185, Title = "WP 10" },
                new() { Altitude = 100, Distance = 213, Title = "WP 11" },
                new() { Altitude = 60, Distance = 164, Title = "WP 12" },
                new() { Altitude = 60, Distance = 108, Title = "WP 13" },
                new() { Altitude = 70, Distance = 321, Title = "WP 14" },
                new() { Altitude = 70, Distance = 232, Title = "WP 15" }
            };
            
            this.WhenAnyValue(_ => _.DisableAll).Subscribe(_ =>
            {
                EnablePolygons = false;
                EnablePolygonsAndAnchors = false;
            }).DisposeItWith(Disposable);
            
            this.WhenAnyValue(_ => _.EnablePolygons).Subscribe(_ =>
            {
                DisableAll = false;
                EnablePolygonsAndAnchors = false;
            }).DisposeItWith(Disposable);
            
            this.WhenAnyValue(_ => _.EnablePolygonsAndAnchors).Subscribe(_ =>
            {
                DisableAll = false;
                EnablePolygons= false;
            }).DisposeItWith(Disposable);
        }
        else
        {
            WayPoints = new AvaloniaList<RoundWayPointItem>();
        }

        
    }

    public bool DisableAll { get; set; } = true;
    
    public bool EnablePolygons { get; set; } = true;

    public bool EnablePolygonsAndAnchors { get; set; } = true;
    
    public AvaloniaList<RoundWayPointItem> WayPoints { get; set; }
}