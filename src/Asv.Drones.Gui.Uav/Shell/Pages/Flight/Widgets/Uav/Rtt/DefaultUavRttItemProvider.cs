using System.ComponentModel.Composition;
using Asv.Mavlink;

namespace Asv.Drones.Gui.Uav;

[Export(typeof(IUavRttItemProvider))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class DefaultUavRttItemProvider : IUavRttItemProvider
{
    public IEnumerable<IUavRttItem> Create(IVehicle vehicle)
    {
        
        yield return new BatteryUavRttViewModel(vehicle);
    }
}