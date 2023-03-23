using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Asv.Drones.Gui.Uav;
using Asv.Mavlink;

namespace Asv.Drones.Gui.Gbs;

[Export(typeof(IUavRttItemProvider))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class DefaultGbsRttItemProvider : IUavRttItemProvider
{
    private readonly ILocalizationService _localizationService;
    
    [ImportingConstructor]
    public DefaultGbsRttItemProvider(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }
    
    public IEnumerable<IUavRttItem> Create(IVehicle vehicle)
    {
        
    }
}