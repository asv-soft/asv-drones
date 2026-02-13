using Asv.Avalonia;

namespace Asv.Drones.Api;

public interface ISupportRemove : IRoutable
{
    ValueTask RemoveAsync(CancellationToken ct);
}
