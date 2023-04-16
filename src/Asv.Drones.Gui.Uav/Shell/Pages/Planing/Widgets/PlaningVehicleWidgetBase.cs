using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Material.Icons;

namespace Asv.Drones.Gui.Uav
{
    public class PlaningVehicleWidgetBase:PlaningWidgetBase
    {
        protected PlaningVehicleWidgetBase():base(new Uri($"fordesigntime://{Guid.NewGuid()}"))
        {
            
        }

        protected PlaningVehicleWidgetBase(IVehicleClient vehicle,string name) : base(new Uri(UriString + $"/{vehicle.FullId}/{name}"))
        {
            Vehicle = vehicle;
        }
        
        protected IVehicleClient Vehicle { get; }
        
        protected override void InternalAfterMapInit(IMap map)
        {
            
        }
    }
   
}