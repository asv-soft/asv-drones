using Asv.Avalonia;
using Microsoft.Extensions.Logging;

namespace Asv.Drones;

public sealed class TypePacketFilterViewModel(
    PacketMessageViewModel pkt,
    IUnitService unitService,
    ILoggerFactory loggerFactory
) : PacketFilterViewModelBase<TypePacketFilterViewModel>(pkt.Type, unitService, loggerFactory)
{
    public override string FilterValue { get; } = pkt.Type;
}
