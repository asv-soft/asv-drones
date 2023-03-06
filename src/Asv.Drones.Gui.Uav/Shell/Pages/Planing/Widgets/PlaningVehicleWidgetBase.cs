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

        protected PlaningVehicleWidgetBase(IVehicle vehicle,string name) : base(new Uri(UriString + $"/{vehicle.FullId}/{name}"))
        {
            Vehicle = vehicle;
        }
        
        protected IVehicle Vehicle { get; }
        
        protected override void InternalAfterMapInit(IMap map)
        {
            
        }
    }
   
}