using Asv.Drones.Gui.Core;
using Asv.Mavlink;

namespace Asv.Drones.Gui.Uav
{
    public interface IUavActionProvider
    {
        public IEnumerable<UavActionActionBase> CreateActions(IVehicleClient vehicle, IMap map);
    }
}