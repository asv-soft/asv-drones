using Asv.Avalonia;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using R3;

namespace Asv.Drones;

public sealed class RtlAction<TWidget>() : FlightWidgetAction<TWidget>("rtl")
    where TWidget : class, IDeviceFlightWidget<IClientDevice>
{
    public const MaterialIconKind ActionIcon = MaterialIconKind.Home;
    public static string ActionDescription => RS.RtlAction_TryCreateAction_Description;

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

        var item = new MenuItem(ActionId, RS.RtlAction_TryCreateAction_Header)
        {
            Icon = ActionIcon,
            Description = ActionDescription,
            Order = 60,
        };
        item.Command = CreateCommand(item, ct => new ValueTask(control.DoRtl(ct)))
            .DisposeItWith(contextDispose);
        return item;
    }
}
