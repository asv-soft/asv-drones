using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;

namespace Asv.Drones.Gui.Gbs;

public class GbsAnchorBase : MapAnchorBase
{
    public const string UriString = FlightPageViewModel.UriString + "/layer/{0}/{1}";
    
    public GbsAnchorBase(IGbsClientDevice device, string name) : base(new Uri(UriString.FormatWith(device.Heartbeat.FullId,name)))
    {
        Device = device;
    }
    
    public IGbsClientDevice Device { get; }
}