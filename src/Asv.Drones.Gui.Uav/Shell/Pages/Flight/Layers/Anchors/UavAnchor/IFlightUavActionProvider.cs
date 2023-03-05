using Asv.Drones.Gui.Core;
using Asv.Drones.Uav;
using Asv.Mavlink;

namespace Asv.Drones.Gui.Uav
{
    public interface IFlightUavActionProvider
    {
        public IEnumerable<UavActionActionBase> CreateActions(IVehicle vehicle, IMap map);
    }
}