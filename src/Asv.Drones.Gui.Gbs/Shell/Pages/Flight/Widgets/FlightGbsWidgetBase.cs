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

        protected FlightGbsWidgetBase(IGbsDevice gbs,Uri uri) : base(uri)
        {
            Gbs = gbs;
        }
        
        protected IGbsDevice Gbs { get; }
        
        protected override void InternalAfterMapInit(IMap map)
        {
            
        }
    }
   
}