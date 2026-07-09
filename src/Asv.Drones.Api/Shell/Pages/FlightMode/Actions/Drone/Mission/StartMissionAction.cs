using Asv.Avalonia;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using R3;

namespace Asv.Drones.Api;

public sealed class StartMissionAction<TTarget>() : DroneMenuAction<TTarget>("start-mission")
    where TTarget : class, IDeviceActionTarget<IClientDevice>
{
    public const string StaticId = "ext.drone.action.start-mission";

    public override string Id => StaticId;

    protected override IMenuItem? TryCreateAction(
        TTarget target,
        CompositeDisposable contextDispose
    )
    {
        var control = target.Device.GetMicroservice<IControlClient>();
        var mission = target.Device.GetMicroservice<IMissionClientEx>();
        if (control is null || mission is null)
        {
            return null;
        }

        var item = CreateMenuItem(RS.StartMissionAction_TryCreateAction_Header);
        item.Icon = MaterialIconKind.MapMarkerPath;
        item.Description = RS.StartMissionAction_TryCreateAction_Description;
        item.Order = 110;
        item.Command = CreateCommand(
                item,
                async ct =>
                {
                    await mission.Download(ct, _ => { });
                    await mission.SetCurrent(0, ct);
                    await control.SetAutoMode(ct);
                }
            )
            .DisposeItWith(contextDispose);
        return item;
    }
}
