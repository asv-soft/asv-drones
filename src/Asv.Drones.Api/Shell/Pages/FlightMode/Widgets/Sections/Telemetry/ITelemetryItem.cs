using Asv.Avalonia;

namespace Asv.Drones.Api;

public interface ITelemetryItem : IViewModel
{
    string ItemId { get; }
}
