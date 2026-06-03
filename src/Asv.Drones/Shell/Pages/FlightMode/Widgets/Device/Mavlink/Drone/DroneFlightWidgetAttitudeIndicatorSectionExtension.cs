using System;
using Asv.Avalonia;
using Asv.Drones.Api;
using Microsoft.Extensions.DependencyInjection;
using R3;

namespace Asv.Drones;

public sealed class DroneFlightWidgetAttitudeIndicatorSectionExtension(IServiceProvider services)
    : IExtensionFor<IDroneFlightWidget>
{
    public void Extend(IDroneFlightWidget context, CompositeDisposable contextDispose)
    {
        var device = context.Device ?? throw new NullReferenceException();
        var section = ActivatorUtilities.CreateInstance<AttitudeIndicatorSectionViewModel>(
            services,
            device
        );

        context.Sections.Add(section);
    }
}
