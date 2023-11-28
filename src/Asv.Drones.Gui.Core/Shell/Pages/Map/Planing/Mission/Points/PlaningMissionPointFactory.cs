using System.ComponentModel.Composition;
using Material.Icons;

namespace Asv.Drones.Gui.Core;

public interface IPlaningMissionPointFactory
{
    IEnumerable<PlaningMissionPointFactoryOption> AddablePoints { get; }
    public PlaningMissionPointViewModel Create(PlaningMissionPointModel point, PlaningMissionViewModel mission);
}

public class PlaningMissionPointFactoryOption
{
    public PlaningMissionPointFactoryOption(string name, MaterialIconKind icon, PlaningMissionPointType type)
    {
        Name = name;
        Type = type;
        Icon = icon;
    }
    public string Name { get; }
    public MaterialIconKind Icon { get; }
    public PlaningMissionPointType Type { get; }
}

[Export(typeof(IPlaningMissionPointFactory))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class PlaningMissionPointFactory : IPlaningMissionPointFactory
{
    private readonly IPlaningMission _svc;
    private readonly ILocalizationService _loc;

    [ImportingConstructor]
    public PlaningMissionPointFactory(IPlaningMission svc, ILocalizationService loc)
    {
        _svc = svc;
        _loc = loc;
        
        AddablePoints = new[]
        {
            new PlaningMissionPointFactoryOption("Take off", MaterialIconKind.FlightTakeoff, PlaningMissionPointType.TakeOff),
            new PlaningMissionPointFactoryOption("Do land", MaterialIconKind.FlightLand, PlaningMissionPointType.DoLand),
            new PlaningMissionPointFactoryOption("Navigation", MaterialIconKind.Location, PlaningMissionPointType.Navigation),
            new PlaningMissionPointFactoryOption("ROI", MaterialIconKind.ImageFilterCenterFocus, PlaningMissionPointType.Roi)
        };
    }
    
    public IEnumerable<PlaningMissionPointFactoryOption> AddablePoints { get; }

    public PlaningMissionPointViewModel Create(PlaningMissionPointModel point, PlaningMissionViewModel mission)
    {
        switch (point.Type)
        {
            case PlaningMissionPointType.TakeOff:
                return new PlaningMissionTakeOffPointViewModel(point, mission, _svc, _loc);
            case PlaningMissionPointType.DoLand:
                return new PlaningMissionLandPointViewModel(point, mission, _svc, _loc);
            case PlaningMissionPointType.Navigation:
                return new PlaningMissionNavigationPointViewModel(point, mission, _svc, _loc);
            case PlaningMissionPointType.Roi:
                return new PlaningMissionRoiPointViewModel(point, mission, _svc, _loc);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}