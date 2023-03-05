using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;

namespace Asv.Drones.Gui.Uav
{
    public class PlaningAnchorBase:MapAnchorBase
    {
        public const string UriString = PlaningPageViewModel.UriString + "/layer/{0}/{1}";
        
        public PlaningAnchorBase(IVehicle vehicle, string name) : base(new Uri(UriString.FormatWith(vehicle.FullId,name)))
        {
            
        }
    }
}