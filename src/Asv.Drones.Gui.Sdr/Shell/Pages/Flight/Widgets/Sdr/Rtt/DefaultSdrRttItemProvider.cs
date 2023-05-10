using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Asv.Mavlink.V2.AsvSdr;

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
    
    public IEnumerable<ISdrRttItem> Create(ISdrClientDevice device, AsvSdrCustomMode mode)
    {
        switch (mode)
        {
            case AsvSdrCustomMode.AsvSdrCustomModeIdle:
                break;
            case AsvSdrCustomMode.AsvSdrCustomModeLlz:
                yield return new SdrRttItemLlzDdmViewModel(device);
                yield return new SdrRttItemLlzSdmViewModel(device);
                yield return new SdrRttItemLlzPowViewModel(device);
                
                yield return new SdrRttItemLlzCrsSdmViewModel(device);
                yield return new SdrRttItemLlzCrsDdmViewModel(device);
                
                yield return new SdrRttItemLlzClrSdmViewModel(device);
                yield return new SdrRttItemLlzClrDdmViewModel(device);
                
                break;
            case AsvSdrCustomMode.AsvSdrCustomModeGp:
                break;
            case AsvSdrCustomMode.AsvSdrCustomModeVor:
                
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }
        
    }
}