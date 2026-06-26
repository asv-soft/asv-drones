using Asv.Avalonia;
using Asv.Avalonia.GeoMap;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using R3;

namespace Asv.Drones;

public sealed class GotoAction<TWidget>() : FlightWidgetAction<TWidget>("goto")
    where TWidget : class, IDeviceFlightWidget<IClientDevice>
{
    public const string StaticId = "ext.flight-widget.action.goto";

    public override string Id => StaticId;

    protected override IMenuItem? TryCreateAction(
        TWidget widget,
        CompositeDisposable contextDispose
    )
    {
        var control = widget.Device.GetMicroservice<IControlClient>();
        var map = widget.FindParentOfType<IFlightModePage>()?.Map;

        if (control is null || map is null)
        {
            return null;
        }

        var position = widget.Device.GetMicroservice<IPositionClientEx>();

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
                        Location = position?.Current.CurrentValue ?? GeoPoint.Zero,
                        Icon = MaterialIconKind.Target,
                        IconSize = 32,
                        IsReadOnly = true,
                    };

                    var point = await map.PickPointAsync(
                        marker,
                        RS.GotoAction_TryCreateAction_Header,
                        ct
                    );
                    if (point is null)
                    {
                        return;
                    }

                    await control.GoTo(point.Value, ct);
                }
            )
            .DisposeItWith(contextDispose);
        return item;
    }
}
