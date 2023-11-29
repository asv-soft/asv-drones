using Asv.Mavlink;

namespace Asv.Drones.Gui.Core;

public interface IPlaningMission
{
    IHierarchicalStore<Guid, PlaningMissionFile> MissionStore { get; }
}