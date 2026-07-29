using Asv.Avalonia;
using Asv.Drones.Api;
using Asv.IO;
using R3;

namespace Asv.Drones.Api;

public sealed class ChangePathVisibilityAction<TTarget>()
    : MissionVisibilityAction<TTarget>(
        "change-path-visibility",
        RS.ChangePathVisibilityAction_TryCreateAction_Header,
        RS.ChangePathVisibilityAction_TryCreateAction_Description,
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
