using System;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;

namespace Asv.Drones;

public class DirectoryItemViewModel : BrowserItemViewModel, IRenamable
{
    private readonly FtpClientService? _ftpService;

    public DirectoryItemViewModel(
        NavigationId id,
        string? parentPath,
        string path,
        string name,
        EntityType type,
        ILoggerFactory loggerFactory,
        FtpClientService? ftpService
    )
        : base(id, parentPath, path, type, loggerFactory)
    {
        _ftpService = ftpService;

        HasChildren = true;
        Name = name;
        FtpEntryType = FtpEntryType.Directory;
    }

    public async ValueTask<string> RenameItemAsync(
        string oldPath,
        string newName,
        CancellationToken ct
    )
    {
        if (string.IsNullOrWhiteSpace(oldPath))
        {
            throw new ArgumentException("Old path is empty", nameof(oldPath));
        }

        if (string.IsNullOrWhiteSpace(newName))
        {
            throw new ArgumentException("New name is empty", nameof(newName));
        }

        string newPath;

        switch (Type)
        {
            case EntityType.Local:
                newPath = LocalFilesMixin.RenameDirectory(oldPath, newName, Logger);
                newPath = BrowserPathRules.EnsureDir(
                    newPath,
                    System.IO.Path.DirectorySeparatorChar
                );
                EditMode = false;
                EditedName.Value = BrowserPathRules.FileNameOf(
                    newPath,
                    System.IO.Path.DirectorySeparatorChar
                );
                break;
            case EntityType.Remote:
                if (_ftpService is null)
                {
                    throw new InvalidOperationException("FTP service is not initialized");
                }
                var parentDir = BrowserPathRules.ParentDirOf(
                    oldPath,
                    MavlinkFtpHelper.DirectorySeparator
                );
                newPath = BrowserPathRules.CombineDir(
                    parentDir,
                    newName,
                    MavlinkFtpHelper.DirectorySeparator
                );
                await _ftpService.RenameAsync(oldPath, newPath, ct).ConfigureAwait(false);
                EditMode = false;
                EditedName.Value = BrowserPathRules.FileNameOf(
                    newPath,
                    MavlinkFtpHelper.DirectorySeparator
                );
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        IsSelected = true;
        return newPath;
    }
}
