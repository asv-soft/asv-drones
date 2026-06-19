using Asv.Avalonia;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using R3;

namespace Asv.Drones;

public sealed class FindDroneAction<TWidget>() : FlightWidgetAction<TWidget>("find-drone")
    where TWidget : class, IDeviceFlightWidget<IClientDevice>
{
    protected override IMenuItem? TryCreateAction(
        TWidget widget,
        CompositeDisposable contextDispose
    )
    {
        var position = widget.Device.GetMicroservice<IPositionClientEx>();
        var map = widget.FindParentOfType<IFlightModePage>()?.Map;
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
