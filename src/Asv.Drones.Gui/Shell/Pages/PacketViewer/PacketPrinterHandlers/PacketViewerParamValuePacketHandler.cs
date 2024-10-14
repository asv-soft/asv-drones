using System.Composition;
using Asv.Mavlink;

namespace Asv.Drones.Gui;
[Export(typeof(IPacketPrinterHandler))]
public class PacketViewerParamValuePacketHandler : ParamSetPacketHandler
{
    
}