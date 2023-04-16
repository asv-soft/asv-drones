using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Asv.Drones.Gui.Uav;
using Asv.Mavlink;

namespace Asv.Drones.Gui.Gbs;

[Export(typeof(IGbsRttItemProvider))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class DefaultGbsRttItemProvider : IGbsRttItemProvider
{
    private readonly ILocalizationService _localizationService;
    
    [ImportingConstructor]
    public DefaultGbsRttItemProvider(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }
    
    public IEnumerable<IGbsRttItem> Create(IGbsClientDevice device)
    {
        yield return new LinkQualityGbsRttViewModel(device);
        yield return new VisibleSatellitesGbsRttViewModel(device);
        yield return new BaseStationModeGbsRttViewModel(device);
        yield return new AccuracyGbsRttViewModel(device, _localizationService);
        yield return new ObservationGbsRttViewModel(device,_localizationService);
        yield return new DGpsRateGbsRttViewModel(device, _localizationService);
    }
}