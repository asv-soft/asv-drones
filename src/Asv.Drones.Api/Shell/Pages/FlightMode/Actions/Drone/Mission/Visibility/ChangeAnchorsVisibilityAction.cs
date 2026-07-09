using Asv.Avalonia;
using Asv.Drones.Api;
using Asv.IO;
using R3;

namespace Asv.Drones.Api;

public sealed class ChangeAnchorsVisibilityAction<TTarget>()
    : MissionVisibilityAction<TTarget>(
        "change-anchors-visibility",
        "Hide/Show mission anchors",
        "Change visibility of the mission anchors",
        120
    )
    where TTarget : class, IViewModel, IDeviceActionTarget<IClientDevice>
{
    public const string StaticId = "ext.drone.action.change-anchors-visibility";

    public override string Id => StaticId;

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
