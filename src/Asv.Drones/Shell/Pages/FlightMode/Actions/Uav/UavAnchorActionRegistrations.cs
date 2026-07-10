using Asv.Avalonia;
using Asv.Drones.Api;

namespace Asv.Drones;

public static class UavAnchorActionRegistrations
{
    extension(ActionsRegistrations.Builder builder)
    {
        public ActionsRegistrations.Builder RegisterUavAnchorActions()
        {
            builder.AppBuilder.Extensions.Register<UavAnchor, AutoModeAction<UavAnchor>>();
            builder.AppBuilder.Extensions.Register<UavAnchor, RefreshMissionAction<UavAnchor>>();
            builder.AppBuilder.Extensions.Register<UavAnchor, StartMissionAction<UavAnchor>>();
            builder.AppBuilder.Extensions.Register<UavAnchor, GuidedAction<UavAnchor>>();
            builder.AppBuilder.Extensions.Register<UavAnchor, TakeOffAction<UavAnchor>>();
            builder.AppBuilder.Extensions.Register<UavAnchor, LandAction<UavAnchor>>();
            builder.AppBuilder.Extensions.Register<UavAnchor, RtlAction<UavAnchor>>();
            builder.AppBuilder.Extensions.Register<UavAnchor, GotoAction<UavAnchor>>();
            builder.AppBuilder.Extensions.Register<UavAnchor, RoiAction<UavAnchor>>();
            builder.AppBuilder.Extensions.Register<UavAnchor, FindDroneAction<UavAnchor>>();
            builder.AppBuilder.Extensions.Register<
                UavAnchor,
                ChangeMissionVisibilityAction<UavAnchor>
            >();
            builder.AppBuilder.Extensions.Register<
                UavAnchor,
                ChangeAnchorsVisibilityAction<UavAnchor>
            >();
            builder.AppBuilder.Extensions.Register<
                UavAnchor,
                ChangePathVisibilityAction<UavAnchor>
            >();
            return builder;
        }
    }
}
