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
    private readonly ILogService _log;

    [ImportingConstructor]
    public DefaultQuickParamsPartProvider(ILocalizationService loc, ILogService log)
    {
        _loc = loc;
        _log = log;
    }

    public IEnumerable<IQuickParamsPart> Create(IVehicleClient vehicle)
    {
        yield return new IdentityQuickParamViewModel(vehicle, _log);
        yield return new SpeedsQuickParamViewModel(vehicle, _loc, _log);
        yield return new ControllerReloadQuickParamViewModel(vehicle, _log);
        yield return new FailSafeQuickParamViewModel(vehicle);
    }
}