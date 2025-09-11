using System;

namespace Asv.Drones;

public sealed class FileBrowserBackend(LocalFilesService local, FtpClientService ftp)
{
    private LocalBrowserItemsOps LocalItemsOps { get; } = new(local);
    private RemoteBrowserItemsOps RemoteItemsOps { get; } = new(ftp);

    public IBrowserItemsOps ResolveOps(FtpBrowserSourceType type)
    {
        return type switch
        {
            FtpBrowserSourceType.Local => LocalItemsOps,
            FtpBrowserSourceType.Remote => RemoteItemsOps,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };
    }
}
