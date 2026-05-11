using Asv.Avalonia;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using R3;

namespace Asv.Drones;

public sealed class LandAction<TWidget>() : FlightWidgetAction<TWidget>("land")
    where TWidget : class, IDeviceFlightWidget<IClientDevice>
{
    public const MaterialIconKind ActionIcon = MaterialIconKind.FlightLand;
    public static string ActionDescription => RS.LandAction_TryCreateAction_Description;

    protected override IMenuItem? TryCreateAction(
        TWidget widget,
        CompositeDisposable contextDispose
    )
    {
        // TODO: check land support
        var control = widget.Device.GetMicroservice<IControlClient>();
        if (control is null)
        {
            return null;
        }

        var item = new MenuItem(ActionId, RS.LandAction_TryCreateAction_Header)
        {
            Icon = ActionIcon,
            Description = ActionDescription,
            Order = DroneActionOrder.Land,
        };
        item.Command = CreateCommand(item, ct => new ValueTask(control.DoLand(ct)))
            .DisposeItWith(contextDispose);
        return item;
    }
}
