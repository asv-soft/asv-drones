using Asv.Avalonia;
using Asv.Drones.Api;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Drones;

public static class TelemetrySectionRegistrations
{
    extension(SectionsRegistrations.Builder builder)
    {
        public SectionsRegistrations.Builder RegisterTelemetrySection()
        {
            // Sections for the drone Widget
            builder.AppBuilder.Extensions.Register<
                IDroneFlightWidget,
                DroneFlightWidgetTelemetrySectionExtension
            >();
            builder.AppBuilder.ViewLocator.RegisterViewFor<ITelemetrySection, DashboardView>();
            builder.AppBuilder.ViewModel.RegisterWithArgs<
                ITelemetrySection,
                TelemetrySectionViewModel,
                TelemetrySectionArgs
            >();
            builder.AppBuilder.ViewLocator.RegisterViewFor<
                TelemetryDisplayItemViewModel,
                TelemetryDisplayItemView
            >();
            builder.AppBuilder.ViewLocator.RegisterViewFor<
                AddTelemetryDisplayItemViewModel,
                AddTelemetryDisplayItemView
            >();

            return builder;
        }
    }
}
