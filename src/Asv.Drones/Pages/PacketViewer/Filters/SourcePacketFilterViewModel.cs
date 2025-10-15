using Asv.Avalonia;
using Microsoft.Extensions.Logging;

namespace Asv.Drones;

public sealed class SourcePacketFilterViewModel(
    PacketMessageViewModel pkt,
    IUnitService unitService,
    ILayoutService layoutService,
    ILoggerFactory loggerFactory
)
    : PacketFilterViewModelBase<SourcePacketFilterViewModel>(
        pkt.Source,
        unitService,
        layoutService,
        loggerFactory
    )
{
    public override string FilterValue { get; } = pkt.Source;
}
