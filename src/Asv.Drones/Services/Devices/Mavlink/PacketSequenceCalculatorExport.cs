using System.Composition;
using Asv.Mavlink;

namespace Asv.Drones;

[Export(typeof(IPacketSequenceCalculator))]
[Shared]
public class PacketSequenceCalculatorExport : PacketSequenceCalculator { }
