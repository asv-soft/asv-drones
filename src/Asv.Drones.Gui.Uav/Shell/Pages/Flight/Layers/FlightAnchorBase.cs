using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;

namespace Asv.Drones.Gui.Uav
{
    
    public class FlightAnchorBase : MapAnchorBase
    {
        public const string UriString = FlightPageViewModel.UriString + "/layer/{0}/{1}";
        
        public FlightAnchorBase(IVehicleClient vehicle, string name) : base(new Uri(UriString.FormatWith(vehicle.Heartbeat.FullId,name)))
        {
            Vehicle = vehicle;
        }
        
        protected IVehicleClient Vehicle { get; }
    }
}