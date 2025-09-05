using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Asv.Drones;

public sealed class LocalFileBrowserOps(LocalFilesService local) : IFileBrowserOps
{
    private readonly LocalFilesService _local =
        local ?? throw new System.ArgumentNullException(nameof(local));

    public char Separator => Path.DirectorySeparatorChar;

    public ValueTask<string> RenameDirectoryAsync(
        string oldPath,
        string newPath,
        ILogger logger,
        CancellationToken ct
    )
    {
        var result = _local.RenameDirectory(oldPath, newPath, logger);
        return ValueTask.FromResult(result);
    }

    public ValueTask<string> RenameFileAsync(
        string oldPath,
        string newPath,
        ILogger logger,
        CancellationToken ct
    )
    {
        var result = _local.RenameFile(oldPath, newPath, logger);
        return ValueTask.FromResult(result);
    }

    public ValueTask RemoveDirectoryAsync(string path, ILogger logger, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        _local.RemoveDirectory(path, true, logger);
        return ValueTask.CompletedTask;
    }

    public ValueTask RemoveFileAsync(string path, ILogger logger, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        _local.RemoveFile(path, logger);
        return ValueTask.CompletedTask;
    }

    public async ValueTask<uint> CalculateCrc32Async(
        string path,
        ILogger logger,
        CancellationToken ct
    )
    {
        return await _local.CalculateCrc32Async(path, ct, logger);
    }
}
