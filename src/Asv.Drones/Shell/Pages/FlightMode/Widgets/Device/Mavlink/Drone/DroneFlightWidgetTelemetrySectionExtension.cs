using System;
using Asv.Avalonia;
using Asv.Drones.Api;
using R3;

namespace Asv.Drones;

public sealed class DroneFlightWidgetTelemetrySectionExtension(IServiceProvider services)
    : IExtensionFor<IDroneFlightWidget>
{
    public const string StaticId = "ext.drone-flight-widget.telemetry-section";

    string Asv.Modeling.ISupportId<string>.Id => StaticId;

    public void Extend(IDroneFlightWidget context, CompositeDisposable contextDispose)
    {
        var device = context.Device ?? throw new NullReferenceException();
        string[] defaultItemIds =
        [
            CurrentFlightModeTelemetryItemFactory.Id,
            BatteryTelemetryItemFactory.Id,
            AltitudeTelemetryItemFactory.Id,
            VelocityTelemetryItemFactory.Id,
            GnssTelemetryItemFactory.Id,
            LinkQualityTelemetryItemFactory.Id,
            HomeAzimuthTelemetryItemFactory.Id,
            MissionTargetTelemetryItemFactory.Id,
        ];

        var vm = services.CreateViewModel<ITelemetrySection, TelemetrySectionArgs>(
            new TelemetrySectionArgs(device, defaultItemIds)
        );

        context.Sections.Add(vm);
    }
}
