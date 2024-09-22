using System.Composition;
using System.Text;
using Asv.Drones.Gui.Api;
using Asv.Mavlink;
using Asv.Mavlink.V2.Common;
using Newtonsoft.Json;

namespace Asv.Drones.Gui;

[Export(typeof(IPacketConverter))]
public class FtpPacketConverter:IPacketConverter
{
    public bool CanConvert(IPacketV2<IPayload> packet)
    {
        return packet.MessageId == FileTransferProtocolPacket.PacketMessageId;
    }

    public string Convert(IPacketV2<IPayload> packet, PacketFormatting formatting = PacketFormatting.None)
    {
        var ftp = packet as FileTransferProtocolPacket;
        var payload = new FtpMessagePayload(ftp.Payload.Payload);
        var str = Encoding.ASCII.GetString(payload.Data).TrimEnd('\0');
        return JsonConvert.SerializeObject(new{
            payload.BurstComplete,
            payload.SequenceNumber,
            payload.ReqOpCodeId,
            payload.Size,
            payload.Offset,
            str,
            
        }, formatting == PacketFormatting.Indented ? Formatting.Indented : Formatting.None);
    }

    public int Order => int.MaxValue/2;
}