using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using DynamicData;

namespace Asv.Drones.Gui.Uav;

[Export(typeof(IQuickParamsPartProvider))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class DefaultQuickParamsPartProvider : IQuickParamsPartProvider
{
    [ImportingConstructor]
    public DefaultQuickParamsPartProvider()
    {
        
    }

    public IEnumerable<IQuickParamsPart> Create(IVehicleClient vehicle)
    {
        yield return new IdentityQuickParamViewModel(vehicle);
        yield return new SpeedsQuickParamViewModel(vehicle);
        yield return new ControllerReloadQuickParamViewModel(vehicle);
        yield return new FailSafeQuickParamViewModel(vehicle);
    }
}