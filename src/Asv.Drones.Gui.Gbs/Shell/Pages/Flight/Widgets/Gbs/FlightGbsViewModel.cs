using System.Collections.ObjectModel;
using System.Windows.Input;
using Asv.Common;
using Asv.Drones.Gui.Core;
using DynamicData;
using Material.Icons;

namespace Asv.Drones.Gui.Gbs;

public class FlightGbsViewModel:FlightGbsWidgetBase
{
    private readonly ReadOnlyObservableCollection<IGbsRttItem> _rttItems;
    public static Uri GenerateUri(IGbsDevice gbs) => FlightGbsWidgetBase.GenerateUri(gbs,"gbs");
    
    public FlightGbsViewModel(IGbsDevice gbsDevice, ILogService log, ILocalizationService localization, IEnumerable<IGbsRttItemProvider> rttItems)
        :base(gbsDevice, GenerateUri(gbsDevice))
    {
        Icon = MaterialIconKind.RouterWireless;
        Title = "Ground base station";
        
        rttItems
            .SelectMany(_ => _.Create(gbsDevice))
            .OrderBy(_=>_.Order)
            .AsObservableChangeSet()
            .AutoRefresh(_=>_.IsVisible)
            .Filter(_=>_.IsVisible)
            .Bind(out _rttItems)
            .DisposeMany()
            .Subscribe()
            .DisposeItWith(Disposable);
        
    }
    
    public ICommand LocateVehicleCommand { get; set; }
    
    public ReadOnlyObservableCollection<IGbsRttItem> RttItems => _rttItems;
    
}