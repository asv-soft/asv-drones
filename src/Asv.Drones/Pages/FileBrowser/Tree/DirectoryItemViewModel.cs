using Asv.Avalonia;
using Asv.Mavlink;

namespace Asv.Drones;

public class DirectoryItemViewModel : BrowserItemViewModel
{
    public DirectoryItemViewModel(NavigationId id, string? parentPath, string path, string? name)
        : base(id, parentPath, path)
    {
        HasChildren = true;
        Header = name;
        FtpEntryType = FtpEntryType.Directory;
    }
}
