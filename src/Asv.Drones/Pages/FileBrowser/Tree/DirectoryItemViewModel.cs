using System;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;

namespace Asv.Drones;

public class DirectoryItemViewModel : BrowserItemViewModel, ISupportRename
{
    public DirectoryItemViewModel(
        NavigationId id,
        string? parentPath,
        string path,
        string name,
        EntityType type,
        FtpClientService? ftpService,
        ILoggerFactory loggerFactory
    )
        : base(id, parentPath, path, type, ftpService, loggerFactory)
    {
        HasChildren = true;
        Name = name;
        FtpEntryType = FtpEntryType.Directory;
    }
}
