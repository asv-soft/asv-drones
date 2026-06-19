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

        var item = CreateMenuItem(RS.GuidedAction_TryCreateAction_Header);
        item.Icon = MaterialIconKind.Controller;
        item.Description = RS.GuidedAction_TryCreateAction_Description;
        item.Order = 30;
        item.Command = CreateCommand(item, ct => new ValueTask(control.SetGuidedMode(ct)))
            .DisposeItWith(contextDispose);
        return item;
    }
}
