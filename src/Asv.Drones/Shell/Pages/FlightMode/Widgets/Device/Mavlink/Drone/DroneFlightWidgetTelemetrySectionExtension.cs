using System;
using Asv.Avalonia;
using Asv.Drones.Api;
using R3;

namespace Asv.Drones;

public sealed class DroneFlightWidgetTelemetrySectionExtension(IServiceProvider services)
    : IExtensionFor<IDroneFlightWidget>
{
    public void Extend(IDroneFlightWidget context, CompositeDisposable contextDispose)
    {
        var device = context.Device ?? throw new NullReferenceException();
        string[] defaultItemIds =
        [
            BatteryTelemetryItemFactory.Id,
            AltitudeTelemetryItemFactory.Id,
            VelocityTelemetryItemFactory.Id,
            AngleTelemetryItemFactory.Id,
        ];

        var vm = services.CreateViewModel<ITelemetrySection, TelemetrySectionArgs>(
            new TelemetrySectionArgs(device, defaultItemIds)
        );

        context.Sections.Add(vm);
    }
}
