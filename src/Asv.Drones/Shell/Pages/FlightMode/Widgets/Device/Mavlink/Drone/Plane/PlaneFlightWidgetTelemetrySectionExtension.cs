using System;
using Asv.Avalonia;
using Asv.Drones.Api;
using R3;

namespace Asv.Drones.Plane;

public sealed class PlaneFlightWidgetTelemetrySectionExtension(IServiceProvider services)
    : IExtensionFor<IPlaneWidget>
{
    public void Extend(IPlaneWidget context, CompositeDisposable contextDispose)
    {
        var device = context.Device ?? throw new NullReferenceException();
        string[] defaultItemIds =
        [
            BatteryTelemetryItemFactory.Id,
            AltitudeTelemetryItemFactory.Id,
            VelocityTelemetryItemFactory.Id,
        ];

        var vm = services.CreateViewModel<ITelemetrySection, TelemetrySectionArgs>(
            new TelemetrySectionArgs(device, defaultItemIds)
        );

        context.Sections.Add(vm);
    }
}
