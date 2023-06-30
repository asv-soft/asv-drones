using Asv.Drones.Gui.Core;
using Asv.Mavlink;

namespace Asv.Drones.Gui.Uav
{
    public class FlightVehicleWidgetBase:FlightWidgetBase
    {
        public static Uri GenerateUri(IVehicleClient vehicle, string name) => new(UriString + $"/{vehicle.Heartbeat.FullId}/{name}");
        
        protected FlightVehicleWidgetBase():base(new Uri($"fordesigntime://{Guid.NewGuid()}"))
        {
            
        }

        protected FlightVehicleWidgetBase(IVehicleClient vehicle,Uri uri) : base(uri)
        {
            Vehicle = vehicle;
        }
        
        protected IVehicleClient Vehicle { get; }
        
        protected override void InternalAfterMapInit(IMap context)
        {
            
        }
    }
   
}