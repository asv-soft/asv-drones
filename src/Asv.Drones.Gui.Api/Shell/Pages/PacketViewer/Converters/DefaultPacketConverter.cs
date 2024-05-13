using System.Composition;
using Asv.Mavlink;
using Newtonsoft.Json;

namespace Asv.Drones.Gui.Api;

/// <summary>
/// Default packet converter. Used when there is no specialized converter for some packet type.
/// </summary>
[Export(typeof(IPacketConverter))]
public class DefaultPacketConverter : IPacketConverter
{
    public int Order => int.MaxValue;

    public bool CanConvert(IPacketV2<IPayload> packet)
    {
        if (packet == null) throw new ArgumentException("Incoming packet was not initialized!");

        return true;
    }

    public string Convert(IPacketV2<IPayload> packet, PacketFormatting formatting = PacketFormatting.None)
    {
        if (packet == null) throw new ArgumentException("Incoming packet was not initialized!");
        if (!CanConvert(packet)) throw new ArgumentException("Converter can not convert incoming packet!");

        string result = string.Empty;

        if (formatting == PacketFormatting.None)
        {
            result = JsonConvert.SerializeObject(packet.Payload, Formatting.None);
        }
        else if (formatting == PacketFormatting.Indented)
        {
            result = JsonConvert.SerializeObject(packet.Payload, Formatting.Indented);
        }
        else
        {
            throw new ArgumentException("Wrong packet formatting!");
        }

        return result;
    }
}