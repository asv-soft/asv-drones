using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;

namespace Asv.Drones;

public interface ISupportRemove : IRoutable
{
    ValueTask RemoveAsync(CancellationToken ct);
}
