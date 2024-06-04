using Asv.Mavlink;

namespace Asv.Drones.Gui.Api;

/// <summary>
/// Represents a planning mission.
/// </summary>
public interface IPlaningMission
{
    /// <summary>
    /// Represents the mission store that stores planning mission files in a hierarchical structure.
    /// </summary>
    /// <remarks>
    /// The MissionStore property provides access to a hierarchical store that is used to store planning mission files.
    /// The store is designed to organize the mission files in a hierarchical manner, allowing easy storage and retrieval
    /// of mission files based on their unique identifier.
    /// </remarks>
    /// <value>
    /// An instance of the IHierarchicalStore interface representing the mission store.
    /// </value>
    IHierarchicalStore<Guid, PlaningMissionFile> MissionStore { get; }
}