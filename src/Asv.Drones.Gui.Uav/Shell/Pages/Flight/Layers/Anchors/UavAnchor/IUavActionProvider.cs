using Asv.Drones.Gui.Core;
using Asv.Mavlink;

namespace Asv.Drones.Gui.Uav
{
    /// <summary>
    /// Represents an interface for a UAV action provider.
    /// </summary>
    public interface IUavActionProvider
    {
        /// <summary>
        /// Creates a collection of UavActionActionBase objects based on the provided vehicle and map.
        /// </summary>
        /// <param name="vehicle">The IVehicleClient instance representing the vehicle.</param>
        /// <param name="map">The IMap instance representing the map.</param>
        /// <returns>A collection of UavActionActionBase objects.</returns>
        public IEnumerable<UavActionActionBase> CreateActions(IVehicleClient vehicle, IMap map);
    }
}