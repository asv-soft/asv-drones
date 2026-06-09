using Asv.Avalonia;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using R3;

namespace Asv.Drones;

public sealed class GuidedAction<TWidget>() : FlightWidgetAction<TWidget>("guided")
    where TWidget : class, IDeviceFlightWidget<IClientDevice>
{
    public const MaterialIconKind ActionIcon = MaterialIconKind.Controller;
    public static string ActionDescription => RS.GuidedAction_TryCreateAction_Description;

    protected override IMenuItem? TryCreateAction(
        TWidget widget,
        CompositeDisposable contextDispose
    )
    {
        var control = widget.Device.GetMicroservice<IControlClient>();
        if (control is null)
        {
            return null;
        }

        var item = new MenuItem(ActionId, RS.GuidedAction_TryCreateAction_Header)
        {
            Icon = ActionIcon,
            Description = ActionDescription,
            Order = 30,
        };
        item.Command = CreateCommand(item, ct => new ValueTask(control.SetGuidedMode(ct)))
            .DisposeItWith(contextDispose);
        return item;
    }
}
