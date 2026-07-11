using Asv.Avalonia;
using Asv.Drones.Api;

namespace Asv.Drones;

public static class DroneWidgetActionRegistrations
{
    extension(ActionsRegistrations.Builder builder)
    {
        public ActionsRegistrations.Builder RegisterDroneWidgetActions()
        {
            // Actions for drone targets
            builder.AppBuilder.Extensions.Register<
                IDroneFlightWidget,
                AutoModeAction<IDroneFlightWidget>
            >();
            builder.AppBuilder.Extensions.Register<
                IDroneFlightWidget,
                RefreshMissionAction<IDroneFlightWidget>
            >();
            builder.AppBuilder.Extensions.Register<
                IDroneFlightWidget,
                StartMissionAction<IDroneFlightWidget>
            >();
            builder.AppBuilder.Extensions.Register<
                IDroneFlightWidget,
                GuidedAction<IDroneFlightWidget>
            >();
            builder.AppBuilder.Extensions.Register<
                IDroneFlightWidget,
                TakeOffAction<IDroneFlightWidget>
            >();
            builder.AppBuilder.Extensions.Register<
                IDroneFlightWidget,
                LandAction<IDroneFlightWidget>
            >();
            builder.AppBuilder.Extensions.Register<
                IDroneFlightWidget,
                RtlAction<IDroneFlightWidget>
            >();
            builder.AppBuilder.Extensions.Register<
                IDroneFlightWidget,
                GotoAction<IDroneFlightWidget>
            >();
            builder.AppBuilder.Extensions.Register<
                IDroneFlightWidget,
                RoiAction<IDroneFlightWidget>
            >();
            builder.AppBuilder.Extensions.Register<
                IDroneFlightWidget,
                FindDroneAction<IDroneFlightWidget>
            >();
            builder.AppBuilder.Extensions.Register<
                IDroneFlightWidget,
                ChangeMissionVisibilityAction<IDroneFlightWidget>
            >();
            builder.AppBuilder.Extensions.Register<
                IDroneFlightWidget,
                ChangeAnchorsVisibilityAction<IDroneFlightWidget>
            >();
            builder.AppBuilder.Extensions.Register<
                IDroneFlightWidget,
                ChangePathVisibilityAction<IDroneFlightWidget>
            >();
            builder.AppBuilder.Extensions.Register<
                IDroneFlightWidget,
                ConfigureTelemetryAction<IDroneFlightWidget>
            >();
            return builder;
        }
    }
}
