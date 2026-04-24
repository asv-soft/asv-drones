using System;
using Asv.Avalonia;
using Asv.Drones.Api;
using Microsoft.Extensions.DependencyInjection;
using R3;

namespace Asv.Drones;

public sealed class DroneFlightWidgetFlightControlSectionExtension(IServiceProvider services)
    : IExtensionFor<IDroneFlightWidget>
{
    public void Extend(IDroneFlightWidget context, CompositeDisposable contextDispose)
    {
        var flightControlViewModel =
            services.GetRequiredKeyedService<FlightControlSectionViewModel>(
                FlightControlSectionViewModel.SectionId
            );
        context.Sections.Add(flightControlViewModel);

        flightControlViewModel.InitWith(context.Device ?? throw new NullReferenceException());
    }
}
