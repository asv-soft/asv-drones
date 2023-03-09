using System.ComponentModel.Composition;

namespace Asv.Drones.Gui.Core
{
    
    [ExportView(typeof(PlaningPageViewModel))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class PlaningPageView:MapPageView
    {
        
    }
}