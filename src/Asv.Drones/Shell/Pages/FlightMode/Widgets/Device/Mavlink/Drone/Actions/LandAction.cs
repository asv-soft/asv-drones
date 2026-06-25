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
    public const string StaticId = "ext.flight-widget.action.land";

    public override string Id => StaticId;

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

        var item = CreateMenuItem(RS.LandAction_TryCreateAction_Header);
        item.Icon = MaterialIconKind.FlightLand;
        item.Description = RS.LandAction_TryCreateAction_Description;
        item.Order = 50;
        item.Command = CreateCommand(item, ct => new ValueTask(control.DoLand(ct)))
            .DisposeItWith(contextDispose);
        return item;
    }
}
