using Asv.Common;
using Asv.Mavlink.V2.Common;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Core;

public record PlaningMissionModel
{
    public List<PlaningMissionPointModel> Points { get; set; } = new();
}

public class PlaningMissionPointModel
{
    [Reactive]
    public int Index { get; set; }
    [Reactive]
    public MavCmd Type { get; set; }
    [Reactive]
    public GeoPoint Location { get; set; }
}