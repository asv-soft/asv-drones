using Asv.Mavlink;

namespace Asv.Drones.Gui.Uav;

public interface IQuickParamsPartProvider
{
    public IEnumerable<IQuickParamsPart> Create(IVehicleClient vehicle);
}