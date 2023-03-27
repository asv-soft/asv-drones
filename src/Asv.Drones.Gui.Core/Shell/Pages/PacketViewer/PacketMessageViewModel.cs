using Asv.Mavlink;
using Newtonsoft.Json;

namespace Asv.Drones.Gui.Core;

public class PacketMessageViewModel
{
    public DateTime DateTime { get; set; }
    public string Source { get; set; }
    public string Message { get; set; }
    public string Type { get; set; }

    public PacketMessageViewModel()
    {
        
    }
    public PacketMessageViewModel(IPacketV2<IPayload> packet)
    {
        DateTime = DateTime.Now;
        Source = $"[{packet.SystemId},{packet.ComponenId}]";
        Message = JsonConvert.SerializeObject(packet.Payload);
        Type = packet.Name;
        Id = Guid.NewGuid();
    }

    public Guid Id { get; }
}