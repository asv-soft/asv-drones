using Asv.Avalonia;
using Microsoft.Extensions.Logging;

namespace Asv.Drones;

public sealed class SourcePacketFilterViewModel(
    PacketMessageViewModel pkt,
    IUnitService unitService,
    ILoggerFactory loggerFactory
) : PacketFilterViewModelBase<SourcePacketFilterViewModel>(pkt.Source, unitService, loggerFactory)
{
    public override string FilterValue { get; } = pkt.Source;
}
