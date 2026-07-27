using Asv.Avalonia;
using Asv.Drones.Api;
using Asv.IO;
using R3;

namespace Asv.Drones.Api;

public sealed class ChangeAnchorsVisibilityAction<TTarget>()
    : MissionVisibilityAction<TTarget>(
        "change-anchors-visibility",
        RS.ChangeAnchorsVisibilityAction_TryCreateAction_Header,
        RS.ChangeAnchorsVisibilityAction_TryCreateAction_Description,
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
