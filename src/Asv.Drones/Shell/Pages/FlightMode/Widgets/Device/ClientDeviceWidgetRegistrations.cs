using Asv.Avalonia;
using Asv.Drones.Api;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Drones;

public static class ClientDeviceWidgetRegistrations
{
    extension(WidgetsRegistrations.Builder builder)
    {
        public WidgetsRegistrations.Builder RegisterClientDeviceWidgets()
        {
            // Factory for client device widgets
            builder.AppBuilder.Services.AddSingleton<
                IClientDeviceWidgetFactory,
                ClientDeviceWidgetFactory
            >();

            // Create widgets for client devices
            builder.AppBuilder.Extensions.Register<
                IFlightModePage,
                FlightModeClientDeviceWidgetExtension
            >();

            // Widget for all drones
            builder.AppBuilder.Services.AddSingleton<
                IClientDeviceWidgetCreationHandler,
                DroneWidgetCreationHandler
            >();
            builder.AppBuilder.ViewLocator.RegisterViewFor<
                DroneFlightWidgetViewModel,
                FlightWidgetView
            >();
            return builder;
        }
    }
}
