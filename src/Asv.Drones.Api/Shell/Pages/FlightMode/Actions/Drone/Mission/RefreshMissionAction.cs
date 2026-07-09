using Asv.Avalonia;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using R3;

namespace Asv.Drones.Api;

public sealed class RefreshMissionAction<TTarget>() : DroneMenuAction<TTarget>("refresh-mission")
    where TTarget : class, IDeviceActionTarget<IClientDevice>
{
    public const string StaticId = "ext.drone.action.refresh-mission";

    public override string Id => StaticId;

    protected override IMenuItem? TryCreateAction(
        TTarget target,
        CompositeDisposable contextDispose
    )
    {
        var mission = target.Device.GetMicroservice<IMissionClientEx>();
        if (mission is null)
        {
            return null;
        }

        var item = CreateMenuItem(RS.RefreshMissionAction_TryCreateAction_Header);
        item.Icon = MaterialIconKind.Refresh;
        item.Description = RS.RefreshMissionAction_TryCreateAction_Description;
        item.Order = 100;
        item.Command = CreateCommand(item, ct => new ValueTask(mission.Download(ct, _ => { })))
            .DisposeItWith(contextDispose);
        return item;
    }
}
