using System;
using Asv.Avalonia;
using Asv.Drones.Api;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public sealed class DroneFlightWidgetExtensionAttitudeIndicatorSection(
    ILoggerFactory loggerFactory,
    IServiceProvider services
) : IExtensionFor<IDroneFlightWidget>
{
    public void Extend(IDroneFlightWidget context, CompositeDisposable contextDispose)
    {
        var section = services.GetRequiredKeyedService<AttitudeIndicatorSectionViewModel>(
            AttitudeIndicatorSectionViewModel.SectionId
        );
        context.Sections.Add(section);
        section.InitWith(context.Device ?? throw new NullReferenceException());
    }
}
