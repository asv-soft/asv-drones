using System;
using System.Collections.Generic;
using Asv.Avalonia;
using Asv.Drones.Api;
using Asv.Mavlink;

namespace Asv.Drones;

public class PacketMessageViewModel : RoutableViewModel
{
    public const string PageId = "packet-message";

    public DateTime DateTime { get; }
    public string Source { get; }
    public int Size { get; }
    public string Message { get; }
    public string Type { get; }
    public string Description { get; }

    private bool _highlight;
    public bool Highlight
    {
        get => _highlight;
        set => SetField(ref _highlight, value);
    }

    public PacketMessageViewModel()
        : base(DesignTime.Id, DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
        DateTime = DateTime.Now;
        Source = "[1,1]";
        Message = "[1000] information";
        Description = "Some description";
        Type = "HEARTBEAT";
        Size = 10;
    }

    public PacketMessageViewModel(MavlinkMessage packet, IPacketConverter converter)
        : base(NavigationId.GenerateByHash(packet.SystemId, packet.ComponentId, packet.Sequence, packet.Id), DesignTime.LoggerFactory)
    {
        DateTime = DateTime.Now;
        Source = $"[{packet.SystemId},{packet.ComponentId}]";
        Message = $"[{packet.Sequence:000}] {converter.Convert(packet)}";
        Description = converter.Convert(packet, PacketFormatting.Indented);
        Type = packet.Name;
        Size = packet.GetByteSize();
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }
}
