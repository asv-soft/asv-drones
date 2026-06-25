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
    public const string StaticId = "ext.flight-widget.action.rtl";

    public override string Id => StaticId;

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

        var item = CreateMenuItem(RS.RtlAction_TryCreateAction_Header);
        item.Icon = MaterialIconKind.Home;
        item.Description = RS.RtlAction_TryCreateAction_Description;
        item.Order = 60;
        item.Command = CreateCommand(item, ct => new ValueTask(control.DoRtl(ct)))
            .DisposeItWith(contextDispose);
        return item;
    }
}
