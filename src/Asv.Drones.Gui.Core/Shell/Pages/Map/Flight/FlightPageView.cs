using System.ComponentModel.Composition;

namespace Asv.Drones.Gui.Core
{
    [ExportView(typeof(FlightPageViewModel))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class FlightPageView:MapPageView
    {
        
    }
}