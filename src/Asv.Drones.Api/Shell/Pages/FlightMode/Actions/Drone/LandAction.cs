using Asv.Avalonia;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using R3;

namespace Asv.Drones.Api;

public sealed class LandAction<TTarget>() : DroneMenuAction<TTarget>("land")
    where TTarget : class, IDeviceActionTarget<IClientDevice>
{
    public const string StaticId = "ext.drone.action.land";

    public override string Id => StaticId;

    protected override IMenuItem? TryCreateAction(
        TTarget target,
        CompositeDisposable contextDispose
    )
    {
        // TODO: check land support
        var control = target.Device.GetMicroservice<IControlClient>();
        if (control is null)
        {
            return null;
        }

        var item = CreateMenuItem(RS.LandAction_TryCreateAction_Header);
        item.Icon = MaterialIconKind.FlightLand;
        item.Description = RS.LandAction_TryCreateAction_Description;
        item.Order = 50;
        item.Command = CreateCommand(item, ct => new ValueTask(control.DoLand(ct)))
            .DisposeItWith(contextDispose);
        return item;
    }
}
