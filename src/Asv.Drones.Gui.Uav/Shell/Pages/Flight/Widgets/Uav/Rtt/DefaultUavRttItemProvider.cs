using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;

namespace Asv.Drones.Gui.Uav;

[Export(typeof(IUavRttItemProvider))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class DefaultUavRttItemProvider : IUavRttItemProvider
{
    private readonly ILocalizationService _localization;
    
    [ImportingConstructor]
    public DefaultUavRttItemProvider(ILocalizationService localization)
    {
        _localization = localization;
    }
    
    public IEnumerable<IUavRttItem> Create(IVehicle vehicle)
    {
        yield return new BatteryUavRttViewModel(vehicle);
        yield return new HomeDistanceUavRttViewModel(vehicle, _localization);
    }
}