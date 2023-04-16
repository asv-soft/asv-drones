using Asv.Mavlink;

namespace Asv.Drones.Gui.Uav;

public interface IUavRttItemProvider
{
    public IEnumerable<IUavRttItem> Create(IVehicleClient vehicle);
}