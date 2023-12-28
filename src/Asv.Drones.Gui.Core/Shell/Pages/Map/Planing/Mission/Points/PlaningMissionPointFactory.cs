using System.ComponentModel.Composition;
using Material.Icons;

namespace Asv.Drones.Gui.Core;

/// <summary>
/// Represents a factory for creating planning mission point view models.
/// </summary>
public interface IPlaningMissionPointFactory
{
    /// <summary>
    /// Creates a PlaningMissionPointViewModel object based on the provided PlaningMissionPointModel and PlaningMissionViewModel.
    /// </summary>
    /// <param name="point">The PlaningMissionPointModel object to create the PlaningMissionPointViewModel from.</param>
    /// <param name="mission">The PlaningMissionViewModel object to associate the PlaningMissionPointViewModel with.</param>
    /// <returns>The created PlaningMissionPointViewModel object.</returns>
    public PlaningMissionPointViewModel Create(PlaningMissionPointModel point, PlaningMissionViewModel mission);
}

/// <summary>
/// Factory class for creating instances of PlaningMissionPointViewModel.
/// </summary>
/// <remarks>
/// This factory class is responsible for creating different types of PlaningMissionPointViewModel objects based on the
/// PlaningMissionPointType provided.
/// </remarks>
[Export(typeof(IPlaningMissionPointFactory))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class PlaningMissionPointFactory : IPlaningMissionPointFactory
{
    /// <summary>
    /// Represents a private read-only variable that holds an instance of the <see cref="IPlaningMission"/> interface.
    /// </summary>
    private readonly IPlaningMission _svc;

    /// <summary>
    /// Represents an instance of a localization service.
    /// </summary>
    private readonly ILocalizationService _loc;

    /// <summary>
    /// Represents a factory for creating planning mission points.
    /// </summary>
    [ImportingConstructor]
    public PlaningMissionPointFactory(IPlaningMission svc, ILocalizationService loc)
    {
        _svc = svc;
        _loc = loc;
    }

    /// <summary>
    /// Creates a new instance of PlaningMissionPointViewModel based on the given PlaningMissionPointModel and PlaningMissionViewModel.
    /// </summary>
    /// <param name="point">The PlaningMissionPointModel object representing the point.</param>
    /// <param name="mission">The PlaningMissionViewModel object representing the mission.</param>
    /// <returns>A new instance of PlaningMissionPointViewModel.</returns>
    /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the point type is not recognized.</exception>
    public PlaningMissionPointViewModel Create(PlaningMissionPointModel point, PlaningMissionViewModel mission)
    {
        switch (point.Type)
        {
            case PlaningMissionPointType.TakeOff:
                return new PlaningMissionTakeOffPointViewModel(point, mission, _svc, _loc);
            case PlaningMissionPointType.DoLand:
                return new PlaningMissionLandPointViewModel(point, mission, _svc, _loc);
            case PlaningMissionPointType.Waypoint:
                return new PlaningMissionNavigationPointViewModel(point, mission, _svc, _loc);
            case PlaningMissionPointType.Roi:
                return new PlaningMissionRoiPointViewModel(point, mission, _svc, _loc);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}