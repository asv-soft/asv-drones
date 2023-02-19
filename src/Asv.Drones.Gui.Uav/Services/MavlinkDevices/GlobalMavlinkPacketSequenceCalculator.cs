using System.ComponentModel.Composition;
using Asv.Mavlink;

namespace Asv.Drones.Gui.Uav
{
    [Export(typeof(IPacketSequenceCalculator))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class GlobalMavlinkPacketSequenceCalculator : PacketSequenceCalculator
    {

    }
}