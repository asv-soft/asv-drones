using Asv.Avalonia;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Material.Icons;
using R3;

namespace Asv.Drones;

public abstract class MissionVisibilityAction<TWidget>(
    string id,
    string header,
    string description,
    int order
) : FlightWidgetAction<TWidget>(id)
    where TWidget : class, IDeviceFlightWidget<IClientDevice>
{
    public abstract override string Id { get; }

    protected override IMenuItem? TryCreateAction(
        TWidget widget,
        CompositeDisposable contextDispose
    )
    {
        var missionClient = widget.Device.GetMicroservice<IMissionClientEx>();
        if (missionClient is null)
        {
            return null;
        }

        var flightMode = widget.FindParentOfType<IFlightModePage>();
        var mission = flightMode
            ?.Map.Anchors.OfType<IMissionContainerAnchor>()
            .FirstOrDefault(anchor => anchor.DeviceId == widget.Device.Id);

        if (mission is null)
        {
            return null;
        }

        var visibility = GetVisibility(mission);
        var item = CreateMenuItem(header);
        item.Icon = GetVisibilityIcon(visibility.CurrentValue);
        item.Description = description;
        item.Order = order;

        visibility
            .ObserveOnUIThreadDispatcher()
            .Subscribe(isVisible => item.Icon = GetVisibilityIcon(isVisible))
            .DisposeItWith(contextDispose);

        item.Command = CreateCommand(
                item,
                _ =>
                {
                    SwitchVisibility(mission);
                    return ValueTask.CompletedTask;
                }
            )
            .DisposeItWith(contextDispose);
        return item;
    }

    protected abstract ReadOnlyReactiveProperty<bool> GetVisibility(
        IMissionContainerAnchor missionContainer
    );

    protected abstract void SwitchVisibility(IMissionContainerAnchor missionContainer);

    private static MaterialIconKind GetVisibilityIcon(bool isVisible)
    {
        return isVisible ? MaterialIconKind.Visibility : MaterialIconKind.VisibilityOff;
    }
}
