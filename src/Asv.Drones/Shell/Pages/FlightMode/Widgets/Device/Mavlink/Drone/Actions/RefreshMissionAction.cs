using Asv.Avalonia;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using R3;

namespace Asv.Drones;

public sealed class RefreshMissionAction<TWidget>() : FlightWidgetAction<TWidget>("refresh-mission")
    where TWidget : class, IDeviceFlightWidget<IClientDevice>
{
    public const MaterialIconKind ActionIcon = MaterialIconKind.Refresh;
    public static string ActionDescription => RS.RefreshMissionAction_TryCreateAction_Description;

    protected override IMenuItem? TryCreateAction(
        TWidget widget,
        CompositeDisposable contextDispose
    )
    {
        var mission = widget.Device.GetMicroservice<IMissionClientEx>();
        if (mission is null)
        {
            return null;
        }

        var item = new MenuItem(ActionId, RS.RefreshMissionAction_TryCreateAction_Header)
        {
            Icon = ActionIcon,
            Description = ActionDescription,
            Order = 100,
        };
        item.Command = CreateCommand(item, ct => new ValueTask(mission.Download(ct, _ => { })))
            .DisposeItWith(contextDispose);
        return item;
    }
}
