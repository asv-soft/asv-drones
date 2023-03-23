using Asv.Drones.Gui.Core;
using Asv.Mavlink;

namespace Asv.Drones.Gui.Gbs
{
    public class FlightGbsWidgetBase:FlightWidgetBase
    {
        public static Uri GenerateUri(IGbsDevice gbs, string name) => new(UriString + $"/{gbs.FullId}/{name}");
        
        protected FlightGbsWidgetBase():base(new Uri($"fordesigntime://{Guid.NewGuid()}"))
        {
            
        }

        protected FlightGbsWidgetBase(IVehicle vehicle,Uri uri) : base(uri)
        {
            Vehicle = vehicle;
        }
        
        protected IVehicle Vehicle { get; }
        
        protected override void InternalAfterMapInit(IMap map)
        {
            
        }
    }
   
}