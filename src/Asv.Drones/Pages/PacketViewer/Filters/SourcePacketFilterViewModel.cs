using System.Collections.Generic;
using Asv.Avalonia;
using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public sealed class SourcePacketFilterViewModel
    : PacketFilterViewModelBase<SourcePacketFilterViewModel>
{
    public SourcePacketFilterViewModel(
        PacketMessageViewModel pkt,
        IUnitService unitService,
        ILoggerFactory loggerFactory
    )
        : base(pkt.Source, unitService, loggerFactory)
    {
        FilterValue = new BindableReactiveProperty<string>(pkt.Source).DisposeItWith(Disposable);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    public override BindableReactiveProperty<string> FilterValue { get; }
}
