using System;
using System.Threading.Tasks;

namespace Asv.Drones;

public sealed class FileBrowserBackend(LocalFilesService local, FtpClientService ftp) : IDisposable
{
    private readonly LocalFilesService _local =
        local ?? throw new ArgumentNullException(nameof(local));

    private readonly FtpClientService _remote = ftp ?? throw new ArgumentNullException(nameof(ftp));

    public async ValueTask<T> UseAsync<T>(
        FtpBrowserSourceType type,
        Func<LocalFilesService, ValueTask<T>> onLocal,
        Func<FtpClientService, ValueTask<T>> onRemote
    )
    {
        return type switch
        {
            FtpBrowserSourceType.Local => await onLocal(_local),
            FtpBrowserSourceType.Remote => await onRemote(_remote),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };
    }

    public void Dispose()
    {
        _remote.Dispose();
    }
}
