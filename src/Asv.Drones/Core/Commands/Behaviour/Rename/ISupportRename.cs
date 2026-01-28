using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;

namespace Asv.Drones;

public interface ISupportRename : IRoutable
{
    ValueTask<string> RenameAsync(string oldValue, string newValue, CancellationToken ct);
}
