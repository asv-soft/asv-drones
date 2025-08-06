using Asv.Avalonia;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;

namespace Asv.Drones;

public class DirectoryItemViewModel : BrowserItemViewModel
{
    public DirectoryItemViewModel(
        NavigationId id,
        string? parentPath,
        string path,
        string? name,
        EntityType type,
        ILoggerFactory loggerFactory
    )
        : base(id, parentPath, path, type, loggerFactory)
    {
        HasChildren = true;
        Header = name;
        FtpEntryType = FtpEntryType.Directory;
    }
}
