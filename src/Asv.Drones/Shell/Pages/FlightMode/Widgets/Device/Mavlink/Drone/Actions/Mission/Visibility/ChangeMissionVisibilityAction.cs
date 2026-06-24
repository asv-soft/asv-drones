using Asv.Drones.Api;
using Asv.IO;
using R3;

namespace Asv.Drones;

public sealed class ChangeMissionVisibilityAction<TWidget>()
    : MissionVisibilityAction<TWidget>(
        "change-mission-visibility",
        "Hide/Show mission",
        "Change visibility of the whole mission",
        119
    )
    where TWidget : class, IDeviceFlightWidget<IClientDevice>
{
    protected override ReadOnlyReactiveProperty<bool> GetVisibility(
        IMissionContainerAnchor missionContainer
    )
    {
        return missionContainer.IsMissionVisible;
    }

    protected override void SwitchVisibility(IMissionContainerAnchor missionContainer)
    {
        missionContainer.SwitchAllVisibility();
    }
}
