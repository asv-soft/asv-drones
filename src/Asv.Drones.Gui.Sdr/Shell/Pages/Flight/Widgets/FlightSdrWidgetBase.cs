using Asv.Drones.Gui.Core;
using Asv.Mavlink;

namespace Asv.Drones.Gui.Sdr
{
    public class FlightSdrWidgetBase:FlightWidgetBase
    {
        public static Uri GenerateUri(ISdrClientDevice gbs, string name) => new(UriString + $"/{gbs.Heartbeat.FullId}/{name}");
        
        protected FlightSdrWidgetBase():base(new Uri($"fordesigntime://{Guid.NewGuid()}"))
        {
            Payload = null!;
        }

        protected FlightSdrWidgetBase(ISdrClientDevice payload,Uri uri) : base(uri)
        {
            Payload = payload;
        }
        
        protected ISdrClientDevice Payload { get; }
        
        protected override void InternalAfterMapInit(IMap map)
        {
            
        }
    }
   
}