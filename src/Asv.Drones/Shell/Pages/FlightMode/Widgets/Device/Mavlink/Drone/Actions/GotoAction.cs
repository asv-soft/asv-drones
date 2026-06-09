using Asv.Avalonia;
using Asv.Avalonia.GeoMap;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using R3;

namespace Asv.Drones;

public sealed class GotoAction<TWidget>(IDialogService dialogService)
    : FlightWidgetAction<TWidget>("goto")
    where TWidget : class, IDeviceFlightWidget<IClientDevice>
{
    public const MaterialIconKind ActionIcon = MaterialIconKind.Target;
    public static string ActionDescription => RS.GotoAction_TryCreateAction_Description;

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

        var position = widget.Device.GetMicroservice<IPositionClientEx>();

        var item = new MenuItem(ActionId, RS.GotoAction_TryCreateAction_Header)
        {
            Icon = ActionIcon,
            Description = ActionDescription,
            Order = 70,
        };
        item.Command = CreateCommand(
                item,
                async ct =>
                {
                    var geoPointDialog = dialogService.GetDialogPrefab<GeoPointDialogPrefab>();

                    var point = await geoPointDialog.ShowDialogAsync(
                        new GeoPointDialogPayload
                        {
                            InitialLocation = position?.Current.CurrentValue ?? GeoPoint.Zero,
                        }
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
