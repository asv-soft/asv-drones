using Asv.Avalonia;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using R3;

namespace Asv.Drones.Api;

public sealed class RtlAction<TTarget>() : DroneMenuAction<TTarget>("rtl")
    where TTarget : class, IDeviceActionTarget<IClientDevice>
{
    public const string StaticId = "ext.drone.action.rtl";

    public override string Id => StaticId;

    protected override IMenuItem? TryCreateAction(
        TTarget target,
        CompositeDisposable contextDispose
    )
    {
        var control = target.Device.GetMicroservice<IControlClient>();
        if (control is null)
        {
            return null;
        }

        var item = CreateMenuItem(RS.RtlAction_TryCreateAction_Header);
        item.Icon = MaterialIconKind.Home;
        item.Description = RS.RtlAction_TryCreateAction_Description;
        item.Order = 60;
        item.Command = CreateCommand(item, ct => new ValueTask(control.DoRtl(ct)))
            .DisposeItWith(contextDispose);
        return item;
    }
}
