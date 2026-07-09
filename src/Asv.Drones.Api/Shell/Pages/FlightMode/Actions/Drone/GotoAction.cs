using Asv.Avalonia;
using Asv.Avalonia.GeoMap;
using Asv.Common;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using R3;

namespace Asv.Drones.Api;

public sealed class GotoAction<TTarget>() : DroneMenuAction<TTarget>("goto")
    where TTarget : class, IViewModel, IDeviceActionTarget<IClientDevice>
{
    public const string StaticId = "ext.drone.action.goto";

    public override string Id => StaticId;

    protected override IMenuItem? TryCreateAction(
        TTarget target,
        CompositeDisposable contextDispose
    )
    {
        var control = target.Device.GetMicroservice<IControlClient>();
        var position = target.Device.GetMicroservice<IPositionClientEx>();
        var map = target.FindParentOfType<IFlightModePage>()?.Map;

        if (control is null || position is null || map is null)
        {
            return null;
        }

        var item = CreateMenuItem(RS.GotoAction_TryCreateAction_Header);
        item.Icon = MaterialIconKind.Target;
        item.Description = RS.GotoAction_TryCreateAction_Description;
        item.Order = 70;
        item.Command = CreateCommand(
                item,
                async ct =>
                {
                    var marker = new MapAnchor("goto.preview")
                    {
                        Location = position.Current.CurrentValue,
                        Icon = MaterialIconKind.Target,
                        IconSize = 32,
                        IsReadOnly = true,
                    };

                    var pointWithoutAlt = await map.PickPointAsync(
                        marker,
                        RS.GotoAction_TryCreateAction_Header,
                        ct
                    );

                    if (pointWithoutAlt is null)
                    {
                        return;
                    }

                    var point = pointWithoutAlt.Value.SetAltitude(
                        position.Current.CurrentValue.Altitude
                    );

                    await control.GoTo(point, ct);
                }
            )
            .DisposeItWith(contextDispose);
        return item;
    }
}
