using Asv.Drones.Gui.Core;
using Asv.Mavlink;

namespace Asv.Drones.Gui.Gbs
{
    public class FlightGbsWidgetBase:FlightWidgetBase
    {
        public static Uri GenerateUri(IGbsClientDevice gbs, string name) => new(UriString + $"/{gbs.Heartbeat.FullId}/{name}");
        
        protected FlightGbsWidgetBase():base(new Uri($"fordesigntime://{Guid.NewGuid()}"))
        {
            
        }

        protected FlightGbsWidgetBase(IGbsClientDevice baseStation,Uri uri) : base(uri)
        {
            BaseStation = baseStation;
        }
        
        protected IGbsClientDevice BaseStation { get; }
        
        protected override void InternalAfterMapInit(IMap context)
        {
            
        }
    }
   
}