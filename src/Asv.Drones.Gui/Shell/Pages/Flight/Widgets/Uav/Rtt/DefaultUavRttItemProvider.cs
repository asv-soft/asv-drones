using System.Collections.Generic;
using System.Composition;
using Asv.Drones.Gui.Api;
using Asv.Mavlink;

namespace Asv.Drones.Gui;

[Export(typeof(IUavRttItemProvider))]
public class DefaultUavRttItemProvider : IUavRttItemProvider
{
    private readonly ILocalizationService _loc;

    [ImportingConstructor]
    public DefaultUavRttItemProvider(ILocalizationService loc)
    {
        _loc = loc;
    }

    public IEnumerable<IUavRttItem> Create(IVehicleClient vehicle)
    {
        yield return new FlightTimeUavRttViewModel(vehicle, _loc);
        yield return new BatteryUavRttViewModel(vehicle);
        yield return new HomeDistanceUavRttViewModel(vehicle, _loc);
        yield return new GpsUavRttViewModel(vehicle, vehicle.Gnss.Main.Info);
        yield return new GpsUavRttViewModel(vehicle, vehicle.Gnss.Additional.Info);
        yield return new VoltageUavRttItemViewModel(vehicle, _loc);
        yield return new CurrentUavRttViewModel(vehicle, _loc);
        yield return new LinkQualityUavRttViewModel(vehicle);
        yield return new CpuLoadUavRttViewModel(vehicle);
        yield return new ConsumedEnergyMAhUavRttViewModel(vehicle, _loc);
    }
}