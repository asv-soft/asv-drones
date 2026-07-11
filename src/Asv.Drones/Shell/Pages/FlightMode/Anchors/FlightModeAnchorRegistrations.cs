using Asv.Avalonia;
using Asv.Drones.Api;

namespace Asv.Drones;

public static class FlightModeAnchorRegistrations
{
    extension(FlightModeRegistrations.Builder builder)
    {
        public FlightModeRegistrations.Builder RegisterAnchors()
        {
            // Anchors
            builder.AppBuilder.Extensions.Register<
                IFlightModePage,
                FlightModeUavAnchorsExtension
            >();
            builder.AppBuilder.Extensions.Register<
                IFlightModePage,
                FlightModeMissionAnchorsExtension
            >();
            return builder;
        }
    }
}
