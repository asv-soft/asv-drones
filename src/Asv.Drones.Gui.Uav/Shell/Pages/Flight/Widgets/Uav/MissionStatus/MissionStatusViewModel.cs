using System.Collections.ObjectModel;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Asv.Mavlink.V2.Common;
using Avalonia.Controls;
using DynamicData;
using DynamicData.PLinq;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Uav.Uav;

public class MissionStatusViewModel : ViewModelBase
{
    private readonly IVehicle _vehicle;
    
    public MissionStatusViewModel() : base(new Uri("designTime://missionstatus"))
    {
        if (Design.IsDesignMode)
        {
            _wayPoints = new ReadOnlyObservableCollection<RoundWayPointItem>(
                new ObservableCollection<RoundWayPointItem>
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
                });
            
            
            Distance = 125;    
        }
    }

    public MissionStatusViewModel(IVehicle vehicle, Uri id, ILocalizationService localization) : base(id)
    {
        _vehicle = vehicle;
        
        _vehicle.MissionItems.Filter(_ => _.Command.Value != MavCmd.MavCmdNavReturnToLaunch).Transform(_ => new RoundWayPointItem(_))
            .Bind(out _wayPoints)
            .Subscribe()
            .DisposeItWith(Disposable);
    }

    private ReadOnlyObservableCollection<RoundWayPointItem> _wayPoints;
    
    [Reactive]
    public bool DisableAll { get; set; }
    [Reactive]
    public bool EnablePolygons { get; set; }
    [Reactive]
    public bool EnablePolygonsAndAnchors { get; set; }
    [Reactive]
    public double Distance { get; set; }

    public ReadOnlyObservableCollection<RoundWayPointItem> WayPoints => _wayPoints;
}