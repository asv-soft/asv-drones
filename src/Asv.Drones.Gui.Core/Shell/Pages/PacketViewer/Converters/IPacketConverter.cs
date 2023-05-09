using Asv.Mavlink;

namespace Asv.Drones.Gui.Core;

public enum PacketFormatting
{
    /// <summary>
    /// One-line formatting
    /// </summary>
    None,
    /// <summary>
    /// Multi-line formatting
    /// </summary>
    Indented
}

public interface IPacketConverter
{
    /// <summary>
    /// Converter order in the list of all converters.
    /// </summary>
    int Order { get; }
    /// <summary>
    /// Checks whether is converter can convert this packet type payload or not.
    /// </summary>
    /// <param name="packet">Packet to convert</param>
    /// <returns>If true - can convert, false if not</returns>
    bool CanConvert(IPacketV2<IPayload> packet);
    /// <summary>
    /// Converts packet's payload to string.
    /// </summary>
    /// <param name="packet">Packet to convert</param>
    /// <param name="formatting">
    /// Formatting of result string.
    /// Needed to create packets with special formatting.
    /// 'None' - by default.
    /// </param>
    /// <returns>String packet representation</returns>
    string Convert(IPacketV2<IPayload> packet, PacketFormatting formatting = PacketFormatting.None);
}