using Asv.Drones.Api;
using Asv.IO;
using R3;

namespace Asv.Drones;

public sealed class ChangePathVisibilityAction<TWidget>()
    : MissionVisibilityAction<TWidget>(
        "change-path-visibility",
        "Hide/Show mission path",
        "Change visibility of the mission path",
        121
    )
    where TWidget : class, IDeviceFlightWidget<IClientDevice>
{
    protected override ReadOnlyReactiveProperty<bool> GetVisibility(IDeviceMissionLayer mission)
    {
        return mission.IsPathVisible;
    }

    protected override void SwitchVisibility(IDeviceMissionLayer mission)
    {
        mission.SwitchPathVisibility();
    }
}
