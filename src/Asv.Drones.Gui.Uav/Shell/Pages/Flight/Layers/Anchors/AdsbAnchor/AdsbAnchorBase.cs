using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;

namespace Asv.Drones.Gui.Uav;

public class AdsbAnchorBase : MapAnchorBase
{
    public const string UriString = FlightPageViewModel.UriString + "/layer/{0}/{1}";
    
    public AdsbAnchorBase(IAdsbClientDevice device, string name) : base(new Uri(UriString.FormatWith(device.Heartbeat.FullId,name)))
    {
        Device = device;
    }
    
    public IAdsbClientDevice Device { get; }
}