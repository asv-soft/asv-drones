using System;
using System.Composition;
using Asv.Drones.Api;
using Asv.Mavlink;
using Newtonsoft.Json;

namespace Asv.Drones;

/// <summary>
/// Default packet converter. Used when there is no specialized converter for some packet type.
/// </summary>
[Export(typeof(IPacketConverter))]
public class DefaultMavlinkPacketConverter : IPacketConverter
{
    public int Order => int.MaxValue;

    public bool CanConvert(MavlinkMessage packet)
    {
        if (packet == null)
        {
            throw new ArgumentException("Incoming packet was not initialized!");
        }

        return true;
    }

    public string Convert(
        MavlinkMessage packet,
        PacketFormatting formatting = PacketFormatting.None
    )
    {
        if (packet == null)
        {
            throw new ArgumentException("Incoming packet was not initialized!");
        }

        if (!CanConvert(packet))
        {
            throw new ArgumentException("Converter can not convert incoming packet!");
        }

        return formatting switch
        {
            PacketFormatting.None => JsonConvert.SerializeObject(
                packet.GetPayload(),
                Formatting.None
            ),
            PacketFormatting.Indented => JsonConvert.SerializeObject(
                packet.GetPayload(),
                Formatting.Indented
            ),
            _ => throw new ArgumentException("Wrong packet formatting!"),
        };
    }
}
