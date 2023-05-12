using Asv.Mavlink;
using Avalonia;

namespace Asv.Drones.Gui.Core;

public class PacketMessageViewModel : AvaloniaObject
{
    public DateTime DateTime { get; set; }
    public string Source { get; set; }
    public int Size { get; set; }
    public string Message { get; set; }
    public string Type { get; set; }

    private bool _highlight;

    public static readonly DirectProperty<PacketMessageViewModel, bool> HighlightProperty = AvaloniaProperty.RegisterDirect<PacketMessageViewModel, bool>(
        nameof(Highlight), o => o.Highlight, (o, v) => o.Highlight = v);

    public bool Highlight
    {
        get => _highlight;
        set => SetAndRaise(HighlightProperty, ref _highlight, value);
    }

    public PacketMessageViewModel()
    {
        
    }
    public PacketMessageViewModel(IPacketV2<IPayload> packet, IPacketConverter converter)
    {
        DateTime = DateTime.Now;
        Source = $"[{packet.SystemId},{packet.ComponentId}]";
        Message = converter.Convert(packet);
        Description = converter.Convert(packet, PacketFormatting.Indented);
        Type = packet.Name;
        Id = Guid.NewGuid();
        Size = packet.GetMaxByteSize();
    }

    public Guid Id { get; }
    public string Description { get; }
}
