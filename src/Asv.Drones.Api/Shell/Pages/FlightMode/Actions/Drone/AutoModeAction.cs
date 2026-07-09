using Asv.Avalonia;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using R3;

namespace Asv.Drones.Api;

public sealed class AutoModeAction<TTarget>() : DroneMenuAction<TTarget>("auto-mode")
    where TTarget : class, IDeviceActionTarget<IClientDevice>
{
    public const string StaticId = "ext.drone.action.auto-mode";

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

        var item = CreateMenuItem(RS.AutoModeAction_TryCreateAction_Header);
        item.Icon = MaterialIconKind.Automatic;
        item.Description = RS.AutoModeAction_TryCreateAction_Description;
        item.Order = 10;
        item.Command = CreateCommand(item, ct => new ValueTask(control.SetAutoMode(ct)))
            .DisposeItWith(contextDispose);
        return item;
    }
}
