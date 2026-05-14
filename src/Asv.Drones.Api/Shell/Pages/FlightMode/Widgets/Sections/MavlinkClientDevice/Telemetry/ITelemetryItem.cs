using Asv.Avalonia;

namespace Asv.Drones.Api;

public interface ITelemetryItem : IRoutable
{
    string ItemId { get; }
    IRoutable Content { get; }
}
