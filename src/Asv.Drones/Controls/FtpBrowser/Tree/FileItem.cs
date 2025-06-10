

using Asv.Avalonia;
using Asv.Mavlink;

namespace Asv.Drones;

public class FileItem : BrowserItem
{
    public FileItem(NavigationId id, string parentPath, string path, string? name, long size)
        : base(id, parentPath, path)
    {
        HasChildren = false;
        Header = name;
        Size = new FileSize(size);
        FtpEntryType = FtpEntryType.File;
    }
}
