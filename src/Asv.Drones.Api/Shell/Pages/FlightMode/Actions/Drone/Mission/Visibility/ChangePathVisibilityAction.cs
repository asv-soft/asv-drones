using Asv.Avalonia;
using Asv.Drones.Api;
using Asv.IO;
using R3;

namespace Asv.Drones.Api;

public sealed class ChangePathVisibilityAction<TTarget>()
    : MissionVisibilityAction<TTarget>(
        "change-path-visibility",
        "Hide/Show mission path",
        "Change visibility of the mission path",
        121
    )
    where TTarget : class, IViewModel, IDeviceActionTarget<IClientDevice>
{
    public const string StaticId = "ext.drone.action.change-path-visibility";

    public override string Id => StaticId;

    protected override ReadOnlyReactiveProperty<bool> GetVisibility(
        IMissionContainerAnchor missionContainer
    )
    {
        return missionContainer.IsPathVisible;
    }

    protected override void SwitchVisibility(IMissionContainerAnchor missionContainer)
    {
        missionContainer.SwitchPathVisibility();
    }
}
