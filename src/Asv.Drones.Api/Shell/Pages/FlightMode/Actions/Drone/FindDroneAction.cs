using Asv.Avalonia;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using R3;

namespace Asv.Drones.Api;

public sealed class FindDroneAction<TTarget>() : DroneMenuAction<TTarget>("find-drone")
    where TTarget : class, IViewModel, IDeviceActionTarget<IClientDevice>
{
    public const string StaticId = "ext.drone.action.find-drone";

    public override string Id => StaticId;

    protected override IMenuItem? TryCreateAction(
        TTarget target,
        CompositeDisposable contextDispose
    )
    {
        var position = target.Device.GetMicroservice<IPositionClientEx>();
        var map = target.FindParentOfType<IFlightModePage>()?.Map;
        if (position is null || map is null)
        {
            return null;
        }

        var item = CreateMenuItem(RS.FindDroneAction_TryCreateAction_Header);
        item.Icon = MaterialIconKind.Crosshairs;
        item.Description = RS.FindDroneAction_TryCreateAction_Description;
        item.Order = 90;
        item.Command = CreateCommand(
                item,
                _ =>
                {
                    map.CenterMap.Value = position.Current.CurrentValue;
                    return ValueTask.CompletedTask;
                }
            )
            .DisposeItWith(contextDispose);
        return item;
    }
}
