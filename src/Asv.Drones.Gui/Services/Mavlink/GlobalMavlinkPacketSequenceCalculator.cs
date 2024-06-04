using System.Composition;
using Asv.Mavlink;

namespace Asv.Drones.Gui
{
    /// <summary>
    /// Packet sequence calculator for all sent mavlink packet form app to devices
    /// </summary>
    [Export(typeof(IPacketSequenceCalculator))]
    [Shared]
    public class GlobalMavlinkPacketSequenceCalculator : PacketSequenceCalculator
    {
    }
}