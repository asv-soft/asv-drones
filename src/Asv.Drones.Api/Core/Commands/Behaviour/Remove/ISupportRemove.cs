using Asv.Avalonia;

namespace Asv.Drones.Api;

public interface ISupportRemove : IViewModel
{
    ValueTask RemoveAsync(CancellationToken ct);
}
