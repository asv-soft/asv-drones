using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Material.Icons;

namespace Asv.Drones.Gui.Uav
{
    public class FlightVehicleWidgetBase:FlightWidgetBase
    {
        public static Uri GenerateUri(IVehicle vehicle, string name) => new(UriString + $"/{vehicle.FullId}/{name}");
        
        protected FlightVehicleWidgetBase():base(new Uri($"fordesigntime://{Guid.NewGuid()}"))
        {
            
        }

        protected FlightVehicleWidgetBase(IVehicle vehicle,Uri uri) : base(uri)
        {
            Vehicle = vehicle;
        }
        
        protected IVehicle Vehicle { get; }
        
        protected override void InternalAfterMapInit(IMap map)
        {
            
        }
    }
   
}