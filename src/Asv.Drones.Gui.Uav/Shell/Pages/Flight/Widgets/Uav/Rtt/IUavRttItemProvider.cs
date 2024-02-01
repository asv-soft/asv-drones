using Asv.Mavlink;

namespace Asv.Drones.Gui.Uav;

/// <summary>
/// Represents a provider for creating UAV RTT items.
/// </summary>
public interface IUavRttItemProvider
{
    /// <summary>
    /// Creates a collection of IUavRttItem objects based on the specified IVehicleClient.
    /// </summary>
    /// <param name="vehicle">The IVehicleClient to create IUavRttItem objects from.</param>
    /// <returns>A collection of IUavRttItem objects obtained from the IVehicleClient.</returns>
    /// <remarks>
    /// This method takes an IVehicleClient as input and creates a collection of IUavRttItem objects based on the provided IVehicleClient.
    /// The returned collection contains IUavRttItem objects which are obtained by processing the data from the IVehicleClient.
    /// </remarks>
    public IEnumerable<IUavRttItem> Create(IVehicleClient vehicle);
}