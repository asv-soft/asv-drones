using System;
using System.Threading;
using System.Threading.Tasks;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;

namespace Asv.Drones;

public sealed class RemoteBrowserItemsOperations(FtpClientService ftp) : IBrowserItemsOperations
{
    private readonly FtpClientService _ftp = ftp ?? throw new ArgumentNullException(nameof(ftp));

    public char Separator => MavlinkFtpHelper.DirectorySeparator;

    public async ValueTask<string> RenameDirectoryAsync(
        string oldPath,
        string newPath,
        ILogger logger,
        CancellationToken ct
    )
    {
        return await _ftp.RenameAsync(oldPath, newPath, ct);
    }

    public async ValueTask<string> RenameFileAsync(
        string oldPath,
        string newPath,
        ILogger logger,
        CancellationToken ct
    )
    {
        return await _ftp.RenameAsync(oldPath, newPath, ct);
    }

    public async ValueTask RemoveDirectoryAsync(string path, ILogger logger, CancellationToken ct)
    {
        await _ftp.RemoveDirectoryAsync(path, true, ct);
    }

    public async ValueTask RemoveFileAsync(string path, ILogger logger, CancellationToken ct)
    {
        await _ftp.RemoveFileAsync(path, ct);
    }

    public async ValueTask CreateDirectoryAsync(string path, ILogger logger, CancellationToken ct)
    {
        await _ftp.CreateDirectoryAsync(path, ct);
    }

    public async ValueTask<uint> CalculateCrc32Async(
        string path,
        ILogger logger,
        CancellationToken ct
    )
    {
        return await _ftp.CalculateCrc32Async(path, ct);
    }
}
