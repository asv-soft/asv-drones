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
    protected override ReadOnlyReactiveProperty<bool> GetVisibility(IDeviceMissionLayer mission)
    {
        return mission.IsVisible;
    }

    protected override void SwitchVisibility(IDeviceMissionLayer mission)
    {
        mission.SwitchAllVisibility();
    }
}
