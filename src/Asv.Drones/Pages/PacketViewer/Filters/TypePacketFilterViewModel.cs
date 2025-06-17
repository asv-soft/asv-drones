using System.Collections.Generic;
using Asv.Avalonia;
using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public sealed class TypePacketFilterViewModel : PacketFilterViewModelBase<TypePacketFilterViewModel>
{
    public TypePacketFilterViewModel(PacketMessageViewModel pkt, IUnitService unitService, ILoggerFactory loggerFactory)
        : base(pkt.Type, unitService, loggerFactory)
    {
        FilterValue = new BindableReactiveProperty<string>(pkt.Type).DisposeItWith(Disposable);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    public override BindableReactiveProperty<string> FilterValue { get; }
}
