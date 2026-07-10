using Asv.Avalonia;
using Asv.Drones.Api;

namespace Asv.Drones;

public static class AttitudeIndicatorSectionRegistrations
{
    extension(SectionsRegistrations.Builder builder)
    {
        public SectionsRegistrations.Builder RegisterAttitudeIndicatorSection()
        {
            builder.AppBuilder.Extensions.Register<
                IDroneFlightWidget,
                DroneFlightWidgetAttitudeIndicatorSectionExtension
            >();
            builder.AppBuilder.ViewLocator.RegisterViewFor<
                AttitudeIndicatorSectionViewModel,
                AttitudeIndicatorSectionView
            >();
            return builder;
        }
    }
}
