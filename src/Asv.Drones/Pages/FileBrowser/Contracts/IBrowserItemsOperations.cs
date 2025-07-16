using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Asv.Drones;

public interface IBrowserItemsOperations
{
    char Separator { get; }

    ValueTask<string> RenameFileAsync(
        string oldPath,
        string newPath,
        ILogger logger,
        CancellationToken ct
    );
    ValueTask<string> RenameDirectoryAsync(
        string oldPath,
        string newPath,
        ILogger logger,
        CancellationToken ct
    );
    ValueTask RemoveDirectoryAsync(string path, ILogger logger, CancellationToken ct);
    ValueTask RemoveFileAsync(string path, ILogger logger, CancellationToken ct);
    ValueTask CreateDirectoryAsync(string path, ILogger logger, CancellationToken ct);
    ValueTask<uint> CalculateCrc32Async(string path, ILogger logger, CancellationToken ct);
}
