using Asv.Avalonia;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using R3;

namespace Asv.Drones.Api;

public sealed class GuidedAction<TTarget>() : DroneMenuAction<TTarget>("guided")
    where TTarget : class, IDeviceActionTarget<IClientDevice>
{
    public const string StaticId = "ext.drone.action.guided";

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

        var item = CreateMenuItem(RS.GuidedAction_TryCreateAction_Header);
        item.Icon = MaterialIconKind.Controller;
        item.Description = RS.GuidedAction_TryCreateAction_Description;
        item.Order = 30;
        item.Command = CreateCommand(item, ct => new ValueTask(control.SetGuidedMode(ct)))
            .DisposeItWith(contextDispose);
        return item;
    }
}
