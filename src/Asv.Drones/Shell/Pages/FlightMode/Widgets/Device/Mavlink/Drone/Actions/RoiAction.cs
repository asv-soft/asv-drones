using Asv.Avalonia;
using Asv.Avalonia.GeoMap;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using R3;

namespace Asv.Drones;

public sealed class RoiAction<TWidget>(IDialogService dialogService)
    : FlightWidgetAction<TWidget>("roi")
    where TWidget : class, IDeviceFlightWidget<IClientDevice>
{
    protected override IMenuItem? TryCreateAction(
        TWidget widget,
        CompositeDisposable contextDispose
    )
    {
        var position = widget.Device.GetMicroservice<IPositionClientEx>();
        if (position is null)
        {
            return null;
        }

        var item = CreateMenuItem(RS.RoiAction_TryCreateAction_Header);
        item.Icon = MaterialIconKind.ImageFilterCenterFocus;
        item.Description = RS.RoiAction_TryCreateAction_Description;
        item.Order = 80;
        item.Command = CreateCommand(
                item,
                async ct =>
                {
                    var geoPointDialog = dialogService.GetDialogPrefab<GeoPointDialogPrefab>();

                    var point = await geoPointDialog.ShowDialogAsync(
                        new GeoPointDialogPayload
                        {
                            InitialLocation = position.Current.CurrentValue,
                        }
                    );
                    if (point is null)
                    {
                        return;
                    }

                    await position.SetRoi(point.Value, ct);
                }
            )
            .DisposeItWith(contextDispose);
        return item;
    }
}
