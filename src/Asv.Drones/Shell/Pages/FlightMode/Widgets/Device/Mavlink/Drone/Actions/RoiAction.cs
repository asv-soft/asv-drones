using Asv.Avalonia;
using Asv.Avalonia.GeoMap;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using R3;

namespace Asv.Drones;

public sealed class RoiAction<TWidget>() : FlightWidgetAction<TWidget>("roi")
    where TWidget : class, IDeviceFlightWidget<IClientDevice>
{
    public const string StaticId = "ext.flight-widget.action.roi";

    public override string Id => StaticId;

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

        var item = CreateMenuItem(RS.RoiAction_TryCreateAction_Header);
        item.Icon = MaterialIconKind.ImageFilterCenterFocus;
        item.Description = RS.RoiAction_TryCreateAction_Description;
        item.Order = 80;
        item.Command = CreateCommand(
                item,
                async ct =>
                {
                    var marker = new MapAnchor("roi.preview")
                    {
                        Location = position.Current.CurrentValue,
                        Icon = MaterialIconKind.ImageFilterCenterFocus,
                        IconSize = 32,
                        IsReadOnly = true,
                    };

                    var point = await map.PickPointAsync(
                        marker,
                        RS.RoiAction_TryCreateAction_Header,
                        ct
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
