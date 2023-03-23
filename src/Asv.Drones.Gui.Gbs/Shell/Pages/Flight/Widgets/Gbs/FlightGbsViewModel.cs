using Asv.Drones.Gui.Core;

namespace Asv.Drones.Gui.Gbs;

public class FlightGbsViewModel
{
    public FlightGbsViewModel(IGbsDevice gbsDevice, ILogService log, ILocalizationService localization, IEnumerable<IGbsRttItemProvider> rttItems)
    {
        
    }

    public static Uri GenerateUri(IGbsDevice gbs) => FlightVehicleWidgetBase.GenerateUri(gbs,"gbs");
}