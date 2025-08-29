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
        FtpBrowserSourceType type,
        ILoggerFactory loggerFactory
    )
        : base(id, parentPath, path, type, loggerFactory)
    {
        HasChildren = true;
        Name = name;
        FtpEntryType = FtpEntryType.Directory;
    }

    public async ValueTask<string> RenameAsync(
        string oldValue,
        string newValue,
        CancellationToken ct
    )
    {
        ArgumentException.ThrowIfNullOrEmpty(oldValue);
        ArgumentException.ThrowIfNullOrEmpty(newValue);

        var sep =
            Type == FtpBrowserSourceType.Remote
                ? MavlinkFtpHelper.DirectorySeparator
                : System.IO.Path.DirectorySeparatorChar;

        var oldPath = FtpBrowserPath.Normalize(oldValue, true, sep);
        var newPath = FtpBrowserPath.Normalize(newValue, true, sep);

        var result = await Backend.UseAsync(
            Type,
            onLocal: local =>
            {
                var newDirPath = local.RenameDirectory(oldPath, newPath, Logger);
                return ValueTask.FromResult(newDirPath);
            },
            onRemote: async ftp =>
            {
                var newDirPath = await ftp.RenameAsync(oldPath, newPath, ct);
                return newDirPath;
            }
        );
        EditMode = false;
        EditedName.Value = FtpBrowserPath.NameOf(result, sep);
        IsSelected = true;
        return newPath;
    }
}
