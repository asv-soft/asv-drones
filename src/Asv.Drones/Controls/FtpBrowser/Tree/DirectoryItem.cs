using Asv.Avalonia;
using Asv.Mavlink;

namespace Asv.Drones;

public class DirectoryItem : BrowserItem
{
    public DirectoryItem(NavigationId id, string? parentPath, string path, string? name)
        : base(id, parentPath, path)
    {
        HasChildren = true;
        Header = name;
        FtpEntryType = FtpEntryType.Directory;
    }
}
