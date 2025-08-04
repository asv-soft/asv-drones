using Asv.Avalonia;
using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public sealed class TypePacketFilterViewModel(
    PacketMessageViewModel pkt,
    IUnitService unitService,
    ILoggerFactory loggerFactory
) : PacketFilterViewModelBase<TypePacketFilterViewModel>(pkt.Type, unitService, loggerFactory)
{
    public override string FilterValue { get; } = pkt.Type;
}
