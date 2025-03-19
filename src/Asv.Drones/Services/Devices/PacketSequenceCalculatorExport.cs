using System.Composition;
using Asv.Mavlink;

namespace Asv.Drones.Api;

[Export(typeof(IPacketSequenceCalculator))]
[Shared]
public class PacketSequenceCalculatorExport : PacketSequenceCalculator
{
    
}