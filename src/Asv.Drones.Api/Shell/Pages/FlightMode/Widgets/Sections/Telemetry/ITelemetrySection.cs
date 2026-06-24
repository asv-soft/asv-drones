using Asv.Avalonia;
using Asv.IO;

namespace Asv.Drones.Api;

public interface ITelemetrySection : IFlightWidgetSection, IDashboard { }

#pragma warning disable SA1313
public sealed record TelemetrySectionArgs(IClientDevice Device);
#pragma warning restore SA1313
