using Asv.Avalonia;
using Asv.Drones.Api;

namespace Asv.Drones;

public static class ActionDialogRegistrations
{
    extension(ActionsRegistrations.Builder builder)
    {
        public ActionsRegistrations.Builder RegisterDialogs()
        {
            builder.AppBuilder.ViewLocator.RegisterViewFor<
                ConfigureTelemetryDialogViewModel,
                ConfigureTelemetryDialogView
            >();
            builder.AppBuilder.ViewLocator.RegisterViewFor<
                SetAltitudeDialogViewModel,
                SetAltitudeDialogView
            >();
            return builder;
        }
    }
}
