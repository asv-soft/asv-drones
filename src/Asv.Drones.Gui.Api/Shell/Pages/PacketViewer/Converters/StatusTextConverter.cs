using System.Composition;
using System.Text;
using Asv.Mavlink;
using Asv.Mavlink.V2.Common;

namespace Asv.Drones.Gui.Api;

/// <summary>
/// StatusText packet converter.
/// </summary>
[Export(typeof(IPacketConverter))]
public class StatusTextConverter : IPacketConverter
{
    public int Order => 0;

    public bool CanConvert(IPacketV2<IPayload> packet)
    {
        if (packet == null) throw new ArgumentException("Incoming packet was not initialized!");

        return packet.Payload is StatustextPayload;
    }

    public string Convert(IPacketV2<IPayload> packet, PacketFormatting formatting = PacketFormatting.None)
    {
        if (packet == null) throw new ArgumentException("Incoming packet was not initialized!");
        if (!CanConvert(packet)) throw new ArgumentException("Converter can not convert incoming packet!");

        StringBuilder sb = new StringBuilder();

        var payload = packet.Payload as StatustextPayload;

        if (formatting == PacketFormatting.None)
        {
            sb.Append("{");
            sb.Append("\"Severity\":");
            sb.Append($"{payload.Severity},");
            sb.Append("\"Text\":");
            sb.Append($"\"{MavlinkTypesHelper.GetString(payload.Text)}\"");
            sb.Append("}");
        }
        else if (formatting == PacketFormatting.Indented)
        {
            sb.Append("{\n");
            sb.Append("\"Severity\": ");
            sb.Append($"{payload.Severity},\n");
            sb.Append("\"Text\": ");
            sb.Append($"\"{MavlinkTypesHelper.GetString(payload.Text)}\"\n");
            sb.Append("}");
        }
        else
        {
            throw new ArgumentException("Wrong packet formatting!");
        }

        return sb.ToString();
    }
}