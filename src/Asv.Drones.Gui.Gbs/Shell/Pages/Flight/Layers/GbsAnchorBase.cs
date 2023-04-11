using Asv.Common;
using Asv.Drones.Gui.Core;

namespace Asv.Drones.Gui.Gbs;

public class GbsAnchorBase : MapAnchorBase
{
    public const string UriString = FlightPageViewModel.UriString + "/layer/{0}/{1}";
    
    public GbsAnchorBase(IGbsDevice gbs, string name) : base(new Uri(UriString.FormatWith(gbs.FullId,name)))
    {
        Gbs = gbs;
    }
    
    protected IGbsDevice Gbs { get; }
}