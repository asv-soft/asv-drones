using System.ComponentModel.Composition;
using Asv.Mavlink;

namespace Asv.Drones.Gui.Uav
{
    /// <summary>
    /// Packet sequence calculator for all sent mavlink packet form app to devices
    /// </summary>
    [Export(typeof(IPacketSequenceCalculator))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class GlobalMavlinkPacketSequenceCalculator : PacketSequenceCalculator
    {

    }
}