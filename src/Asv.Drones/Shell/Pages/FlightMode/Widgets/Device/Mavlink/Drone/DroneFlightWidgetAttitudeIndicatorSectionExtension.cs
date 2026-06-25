using System;
using Asv.Avalonia;
using Asv.Drones.Api;
using Microsoft.Extensions.DependencyInjection;
using R3;

namespace Asv.Drones;

public sealed class DroneFlightWidgetAttitudeIndicatorSectionExtension(IServiceProvider services)
    : IExtensionFor<IDroneFlightWidget>
{
    public const string StaticId = "ext.drone-flight-widget.attitude-indicator-section";

    string Asv.Modeling.ISupportId<string>.Id => StaticId;

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
