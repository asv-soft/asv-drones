using Asv.Common;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Core;

public record PlaningMissionModel
{
    public List<PlaningMissionPointModel> Points { get; set; } = new();
}

public enum PlaningMissionPointType
{
    // !!! DON'T CHANGE ORDER. ONLY ADD ITEMS !!!
    TakeOff,
    DoLand,
    Navigation,
    Roi
    // !!! DON'T CHANGE ORDER. ONLY ADD ITEMS !!!
}

public class PlaningMissionPointModel
{
    [Reactive]
    public int Index { get; set; }
    [Reactive]
    public PlaningMissionPointType Type { get; set; }
    [Reactive]
    public GeoPoint Location { get; set; }
}