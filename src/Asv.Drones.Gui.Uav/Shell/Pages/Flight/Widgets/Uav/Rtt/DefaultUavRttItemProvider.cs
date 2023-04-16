using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;

namespace Asv.Drones.Gui.Uav;

[Export(typeof(IUavRttItemProvider))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class DefaultUavRttItemProvider : IUavRttItemProvider
{
    private readonly ILocalizationService _localizationService;
    
    [ImportingConstructor]
    public DefaultUavRttItemProvider(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }
    
    public IEnumerable<IUavRttItem> Create(IVehicleClient vehicle)
    {
        yield return new FlightTimeUavRttViewModel(vehicle, _localizationService);
        yield return new BatteryUavRttViewModel(vehicle);
        yield return new HomeDistanceUavRttViewModel(vehicle, _localizationService);
        yield return new GpsUavRttViewModel(vehicle, vehicle.Gnss.Main.Info);
        yield return new GpsUavRttViewModel(vehicle, vehicle.Gnss.Additional.Info);
        yield return new VoltageUavRttItemViewModel(vehicle, _localizationService);
        yield return new CurrentUavRttViewModel(vehicle, _localizationService);
        yield return new LinkQualityUavRttViewModel(vehicle);
        yield return new CpuLoadUavRttViewModel(vehicle);
    }
}