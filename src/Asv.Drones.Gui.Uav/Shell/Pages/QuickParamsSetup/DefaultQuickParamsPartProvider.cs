using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using DynamicData;

namespace Asv.Drones.Gui.Uav;

[Export(typeof(IQuickParamsPartProvider))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class DefaultQuickParamsPartProvider : IQuickParamsPartProvider
{
    private readonly ILocalizationService _loc;

    [ImportingConstructor]
    public DefaultQuickParamsPartProvider(ILocalizationService loc)
    {
        _loc = loc;
    }

    public IEnumerable<IQuickParamsPart> Create(IVehicleClient vehicle)
    {
        yield return new IdentityQuickParamViewModel(vehicle);
        yield return new SpeedsQuickParamViewModel(vehicle, _loc);
        yield return new ControllerReloadQuickParamViewModel(vehicle);
        yield return new FailSafeQuickParamViewModel(vehicle);
    }
}