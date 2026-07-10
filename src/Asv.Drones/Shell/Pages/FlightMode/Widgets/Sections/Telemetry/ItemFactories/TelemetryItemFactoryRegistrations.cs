using Asv.Drones.Api;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Drones;

public static class TelemetryItemFactoryRegistrations
{
    extension(SectionsRegistrations.Builder builder)
    {
        public SectionsRegistrations.Builder RegisterTelemetryItemFactories()
        {
            builder.AppBuilder.Services.AddSingleton<
                ITelemetryItemFactory,
                AltitudeTelemetryItemFactory
            >();
            builder.AppBuilder.Services.AddSingleton<
                ITelemetryItemFactory,
                MslAltitudeTelemetryItemFactory
            >();
            builder.AppBuilder.Services.AddSingleton<
                ITelemetryItemFactory,
                BatteryTelemetryItemFactory
            >();

            builder.AppBuilder.Services.AddSingleton<
                ITelemetryItemFactory,
                HorizontalVelocityTelemetryItemFactory
            >();
            builder.AppBuilder.Services.AddSingleton<
                ITelemetryItemFactory,
                VerticalVelocityTelemetryItemFactory
            >();
            builder.AppBuilder.Services.AddSingleton<
                ITelemetryItemFactory,
                AngleTelemetryItemFactory
            >();
            builder.AppBuilder.Services.AddSingleton<
                ITelemetryItemFactory,
                HeadingTelemetryItemFactory
            >();
            builder.AppBuilder.Services.AddSingleton<
                ITelemetryItemFactory,
                HomeAzimuthTelemetryItemFactory
            >();
            builder.AppBuilder.Services.AddSingleton<
                ITelemetryItemFactory,
                CurrentFlightModeTelemetryItemFactory
            >();
            builder.AppBuilder.Services.AddSingleton<
                ITelemetryItemFactory,
                TimeInAirTelemetryItemFactory
            >();
            builder.AppBuilder.Services.AddSingleton<
                ITelemetryItemFactory,
                AzimuthTelemetryItemFactory
            >();

            builder.AppBuilder.Services.AddSingleton<
                ITelemetryItemFactory,
                LinkQualityTelemetryItemFactory
            >();
            builder.AppBuilder.Services.AddSingleton<
                ITelemetryItemFactory,
                GnssSatellitesTelemetryItemFactory
            >();
            builder.AppBuilder.Services.AddSingleton<
                ITelemetryItemFactory,
                GnssModeTelemetryItemFactory
            >();
            builder.AppBuilder.Services.AddSingleton<
                ITelemetryItemFactory,
                GnssHdopTelemetryItemFactory
            >();
            builder.AppBuilder.Services.AddSingleton<
                ITelemetryItemFactory,
                GnssVdopTelemetryItemFactory
            >();
            builder.AppBuilder.Services.AddSingleton<
                ITelemetryItemFactory,
                MissionTotalDistanceTelemetryItemFactory
            >();
            builder.AppBuilder.Services.AddSingleton<
                ITelemetryItemFactory,
                MissionDistanceTelemetryItemFactory
            >();
            builder.AppBuilder.Services.AddSingleton<
                ITelemetryItemFactory,
                MissionProgressTelemetryItemFactory
            >();
            builder.AppBuilder.Services.AddSingleton<
                ITelemetryItemFactory,
                HomeDistanceTelemetryItemFactory
            >();
            builder.AppBuilder.Services.AddSingleton<
                ITelemetryItemFactory,
                MissionTargetTelemetryItemFactory
            >();
            return builder;
        }
    }
}
