using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Avalonia.Collections;

namespace Asv.Drones.Gui.Uav;

public class MissionStatusViewModel : ViewModelBase
{
    private readonly IVehicle _vehicle;
    private readonly ILocalizationService _localization;

    public MissionStatusViewModel() : base(new Uri("designTime://missionstatus"))
    {
        
    }

    public MissionStatusViewModel(IVehicle vehicle, Uri id, ILocalizationService localization) : base(id)
    {
        _vehicle = vehicle;
        _localization = localization;
    }
    
    public IEnumerable<RoundWayPointItem> RoundItems => new AvaloniaList<RoundWayPointItem>()
    {
        new() { Altitude = 50, Distance = 0, Title = "WP 0"},
        new() { Altitude = 50, Distance = 130, Title = "WP 1"},
        new() { Altitude = 100, Distance = 185, Title = "WP 2"},
        new() { Altitude = 100, Distance = 213, Title = "WP 3"},
        new() { Altitude = 60, Distance = 164, Title = "WP 4"},
        new() { Altitude = 60, Distance = 108, Title = "WP 5"},
        new() { Altitude = 70, Distance = 321, Title = "WP 6"},
        new() { Altitude = 70, Distance = 232, Title = "WP 7"}
    };
}