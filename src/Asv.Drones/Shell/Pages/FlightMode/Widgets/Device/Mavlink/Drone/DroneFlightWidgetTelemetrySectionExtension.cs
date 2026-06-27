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

        var vm = services.CreateViewModel<ITelemetrySection, TelemetrySectionArgs>(
            new TelemetrySectionArgs(device)
        );

        context.Sections.Add(vm);
    }
}
