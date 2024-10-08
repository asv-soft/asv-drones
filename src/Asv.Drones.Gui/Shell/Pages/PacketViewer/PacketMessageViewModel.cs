using System;
using Asv.Drones.Gui.Api;
using Asv.Mavlink;
using Avalonia;

namespace Asv.Drones.Gui;

public class PacketMessageViewModel : AvaloniaObject
{
    public DateTime DateTime { get; set; }
    public string Source { get; set; }
    public int Size { get; set; }
    public string Message { get; set; }
    public string Type { get; set; }

    private bool _highlight;

    public static readonly DirectProperty<PacketMessageViewModel, bool> HighlightProperty =
        AvaloniaProperty.RegisterDirect<PacketMessageViewModel, bool>(
            nameof(Highlight), o => o.Highlight, (o, v) => o.Highlight = v);

    public bool Highlight
    {
        get => _highlight;
        set => SetAndRaise(HighlightProperty, ref _highlight, value);
    }

    public PacketMessageViewModel()
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public PacketMessageViewModel(IPacketV2<IPayload> packet, string packetMsg, string packetDescription )
    {
        DateTime = DateTime.Now;
        Source = $"[{packet.SystemId},{packet.ComponentId}]";
        Message = $"[{packet.Sequence:000}] {packetMsg}";
        Description = packetDescription;
        Type = packet.Name;
        Id = Guid.NewGuid();
        Size = packet.GetByteSize();
    }

    public Guid Id { get; }
    public string Description { get; }
}