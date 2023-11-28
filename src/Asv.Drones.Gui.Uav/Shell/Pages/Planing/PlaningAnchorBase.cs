using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;

namespace Asv.Drones.Gui.Uav
{
    public class PlaningAnchorBase : MapAnchorBase
    {
        public const string UriString = PlaningPageViewModel.UriString + "/layer/{0}/{1}";
        
        public PlaningAnchorBase(IVehicleClient vehicle, string name) : base(new Uri(UriString.FormatWith(vehicle.Heartbeat.FullId,name)))
        {
            Vehicle = vehicle;
        }
        
        public IVehicleClient Vehicle { get; }
    }
}