using System.ComponentModel.Composition;
using Asv.Cfg;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Asv.Mavlink.V2.AsvSdr;

namespace Asv.Drones.Gui.Sdr;

[Export(typeof(ISdrRttProvider))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class DefaultSdrRttProvider : ISdrRttProvider
{
    private readonly ILocalizationService _loc;
    private readonly ILogService _log;
    private readonly IConfiguration _configuration;

    [ImportingConstructor]
    public DefaultSdrRttProvider(ILocalizationService loc, ILogService log, IConfiguration configuration)
    {
        _loc = loc;
        _log = log;
        _configuration = configuration;
    }
    
    public SdrRttViewModelBase Create(ISdrClientDevice device, AsvSdrCustomMode mode)
    {
        switch (mode)
        {
            case AsvSdrCustomMode.AsvSdrCustomModeIdle:
                return null;
            case AsvSdrCustomMode.AsvSdrCustomModeLlz:
                return new LlzSdrRttViewModel(device, _log, _loc, _configuration);
            case AsvSdrCustomMode.AsvSdrCustomModeGp:
                return null;
            case AsvSdrCustomMode.AsvSdrCustomModeVor:
                return null;
            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }
    }
}