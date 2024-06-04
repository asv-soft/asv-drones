using Asv.Mavlink;

namespace Asv.Drones.Gui.Api;

/// <summary>
/// Represents the formatting options for packet data.
/// </summary>
public enum PacketFormatting
{
    /// <summary>
    /// One-line formatting
    /// </summary>
    None,

    /// <summary>
    /// Represents the formatting options for packet content.
    /// </summary>
    Indented
}

/// <summary>
/// Represents an interface for converting packet payloads to string representation.
/// </summary>
public interface IPacketConverter
{
    /// <summary>
    /// Gets the order of the converter in the list of all converters.
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Checks whether the converter can convert the payload of a given packet.
    /// </summary>
    /// <param name="packet">The packet to convert</param>
    /// <returns>Returns true if the converter can convert the payload, false otherwise</returns>
    bool CanConvert(IPacketV2<IPayload> packet);

    /// <summary>
    /// Converts packet's payload to string.
    /// </summary>
    /// <param name="packet">The packet to convert.</param>
    /// <param name="formatting">
    /// The formatting of the result string. This is optional and is used to create packets with special formatting. The default value is 'None'.
    /// </param>
    /// <returns>
    /// A string representation of the packet.
    /// </returns>
    string Convert(IPacketV2<IPayload> packet, PacketFormatting formatting = PacketFormatting.None);
}