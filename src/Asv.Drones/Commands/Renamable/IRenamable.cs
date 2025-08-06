using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;

namespace Asv.Drones;

public interface IRenamable : IRoutable
{
    ValueTask<string> RenameItemAsync(string oldPath, string newName, CancellationToken ct);
}
