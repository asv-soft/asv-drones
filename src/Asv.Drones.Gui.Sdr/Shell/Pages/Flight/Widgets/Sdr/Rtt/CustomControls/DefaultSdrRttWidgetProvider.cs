using System.ComponentModel.Composition;
using Asv.Cfg;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Asv.Mavlink.V2.AsvSdr;

namespace Asv.Drones.Gui.Sdr;

[Export(typeof(ISdrRttWidgetProvider))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class DefaultSdrRttWidgetProvider : ISdrRttWidgetProvider
{
    private readonly ILocalizationService _loc;
    private readonly ILogService _log;
    private readonly IConfiguration _configuration;

    [ImportingConstructor]
    public DefaultSdrRttWidgetProvider(ILocalizationService loc, ILogService log, IConfiguration configuration)
    {
        _loc = loc;
        _log = log;
        _configuration = configuration;
    }
    
    public ISdrRttWidget Create(ISdrClientDevice device, AsvSdrCustomMode mode)
    {
        switch (mode)
        {
            case AsvSdrCustomMode.AsvSdrCustomModeIdle:
                return null;
            case AsvSdrCustomMode.AsvSdrCustomModeLlz:
                return new LlzSdrRttViewModel(device, _log, _loc, _configuration);
            case AsvSdrCustomMode.AsvSdrCustomModeGp:
                return new GpSdrRttViewModel(device, _log, _loc, _configuration);
            case AsvSdrCustomMode.AsvSdrCustomModeVor:
                return new VorSdrRttViewModel(device, _log, _loc, _configuration);
            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }
    }
}