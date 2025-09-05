using System;

namespace Asv.Drones;

public sealed class FileBrowserBackend(LocalFilesService local, FtpClientService ftp)
{
    private LocalFileBrowserOps LocalOps { get; } = new(local);
    private RemoteFileBrowserOps RemoteOps { get; } = new(ftp);

    public IFileBrowserOps ResolveOps(FtpBrowserSourceType type)
    {
        return type switch
        {
            FtpBrowserSourceType.Local => LocalOps,
            FtpBrowserSourceType.Remote => RemoteOps,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };
    }
}
