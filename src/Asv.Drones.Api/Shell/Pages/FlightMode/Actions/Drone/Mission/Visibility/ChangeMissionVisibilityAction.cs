using Asv.Avalonia;
using Asv.Drones.Api;
using Asv.IO;
using R3;

namespace Asv.Drones.Api;

public sealed class ChangeMissionVisibilityAction<TTarget>()
    : MissionVisibilityAction<TTarget>(
        "change-mission-visibility",
        "Hide/Show mission",
        "Change visibility of the whole mission",
        119
    )
    where TTarget : class, IViewModel, IDeviceActionTarget<IClientDevice>
{
    public const string StaticId = "ext.drone.action.change-mission-visibility";

    public override string Id => StaticId;

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
