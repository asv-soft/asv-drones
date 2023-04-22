using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;

namespace Asv.Drones.Gui.Sdr;

[Export(typeof(ISdrRttItemProvider))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class DefaultSdrRttItemProvider : ISdrRttItemProvider
{
    private readonly ILocalizationService _localizationService;
    
    [ImportingConstructor]
    public DefaultSdrRttItemProvider(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }
    
    public IEnumerable<ISdrRttItem> Create(ISdrClientDevice device)
    {
        yield break;
    }
}