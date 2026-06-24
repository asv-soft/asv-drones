using Asv.Drones.Api;
using Asv.IO;
using R3;

namespace Asv.Drones;

public sealed class ChangeAnchorsVisibilityAction<TWidget>()
    : MissionVisibilityAction<TWidget>(
        "change-anchors-visibility",
        "Hide/Show mission anchors",
        "Change visibility of the mission anchors",
        120
    )
    where TWidget : class, IDeviceFlightWidget<IClientDevice>
{
    protected override ReadOnlyReactiveProperty<bool> GetVisibility(
        IMissionContainerAnchor missionContainer
    )
    {
        return missionContainer.IsAnchorsVisible;
    }

    protected override void SwitchVisibility(IMissionContainerAnchor missionContainer)
    {
        missionContainer.SwitchAnchorsVisibility();
    }
}
