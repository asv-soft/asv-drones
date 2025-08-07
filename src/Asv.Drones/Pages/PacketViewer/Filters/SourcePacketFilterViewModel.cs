using Asv.Avalonia;
using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public sealed class SourcePacketFilterViewModel(
    PacketMessageViewModel pkt,
    IUnitService unitService,
    ILoggerFactory loggerFactory
) : PacketFilterViewModelBase<SourcePacketFilterViewModel>(pkt.Source, unitService, loggerFactory)
{
    public override string FilterValue { get; } = pkt.Source;
}
