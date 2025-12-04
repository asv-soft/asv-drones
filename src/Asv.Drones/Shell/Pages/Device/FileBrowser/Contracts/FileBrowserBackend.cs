using System;

namespace Asv.Drones;

public sealed class FileBrowserBackend(LocalFilesService local, FtpClientService ftp)
{
    private LocalBrowserItemsOperations LocalItemsOperations { get; } = new(local);
    private RemoteBrowserItemsOperations RemoteItemsOperations { get; } = new(ftp);

    public IBrowserItemsOperations ResolveOps(FtpBrowserSourceType type)
    {
        return type switch
        {
            FtpBrowserSourceType.Local => LocalItemsOperations,
            FtpBrowserSourceType.Remote => RemoteItemsOperations,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };
    }
}
