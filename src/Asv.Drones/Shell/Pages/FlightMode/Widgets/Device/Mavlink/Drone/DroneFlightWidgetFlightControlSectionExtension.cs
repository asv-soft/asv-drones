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
        var device = context.Device ?? throw new NullReferenceException();
        var vm = ActivatorUtilities.CreateInstance<FlightControlSectionViewModel>(services, device);

        context.Sections.Add(vm);
    }
}
