using Asv.Mavlink;
using Newtonsoft.Json;

namespace Asv.Drones.Gui.Core;

public class PacketMessageViewModel
{
    public string FilterId { get; set; }
    public DateTime DateTime { get; set; }
    public string Source { get; set; }
    public string Message { get; set; }
    public string Type { get; set; }

    public PacketMessageViewModel(IPacketV2<IPayload> packet)
    {
        DateTime = DateTime.Now;
        Source = $"[{packet.SystemId},{packet.ComponenId}]";
        Message = JsonConvert.SerializeObject(packet.Payload);
        Type = packet.Name;
        FilterId = Source + Type;
        Id = Guid.NewGuid();
    }

    public Guid Id { get; }
}