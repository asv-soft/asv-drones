using System.Collections.ObjectModel;
using Asv.Cfg;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using DynamicData;
using Material.Icons;

namespace Asv.Drones.Gui.Sdr;

public class FlightSdrViewModel:FlightSdrWidgetBase
{
    private readonly ReadOnlyObservableCollection<ISdrRttItem> _rttItems;
    private readonly ILogService _logService;
    private readonly ILocalizationService _loc;
    private readonly IConfiguration _configuration;
    public static Uri GenerateUri(ISdrClientDevice sdr) => FlightSdrWidgetBase.GenerateUri(sdr,"sdr");

    public FlightSdrViewModel()
    {
        
    }
    
    public FlightSdrViewModel(ISdrClientDevice payload, ILogService log, ILocalizationService loc, IConfiguration configuration, IEnumerable<ISdrRttItemProvider> rttItems)
        :base(payload, GenerateUri(payload))
    {
        _logService = log;
        _loc = loc;
        _configuration = configuration;
        
        Icon = MaterialIconKind.Memory;
        Title = "Payload";
        Location = WidgetLocation.Right;
        
        rttItems
            .SelectMany(_ => _.Create(payload))
            .OrderBy(_=>_.Order)
            .AsObservableChangeSet()
            .AutoRefresh(_=>_.IsVisible)
            .Filter(_=>_.IsVisible)
            .Bind(out _rttItems)
            .DisposeMany()
            .Subscribe()
            .DisposeItWith(Disposable);
       
    }
    
    public ReadOnlyObservableCollection<ISdrRttItem> RttItems => _rttItems;
       
}