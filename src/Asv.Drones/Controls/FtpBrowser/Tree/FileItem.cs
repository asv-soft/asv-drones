

using Asv.Avalonia;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;

namespace Asv.Drones;

public class FileItem : BrowserItem
{
    public FileItem(NavigationId id, string parentPath, string path, string? name, long size, ILoggerFactory loggerFactory)
        : base(id, parentPath, path, loggerFactory)
    {
        HasChildren = false;
        Header = name;
        Size = new FileSize(size);
        FtpEntryType = FtpEntryType.File;
    }
}
