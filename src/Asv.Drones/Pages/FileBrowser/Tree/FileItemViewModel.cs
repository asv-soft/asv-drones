using System;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public class FileItemViewModel : BrowserItemViewModel, IRenamable
{
    private readonly FtpClientService? _ftpService;

    public FileItemViewModel(
        NavigationId id,
        string parentPath,
        string path,
        string name,
        long size,
        EntityType type,
        ILoggerFactory loggerFactory,
        FtpClientService? ftpService
    )
        : base(id, parentPath, path, type, loggerFactory)
    {
        _ftpService = ftpService;

        HasChildren = false;
        Name = name;
        Size = new FileSize(size);
        FtpEntryType = FtpEntryType.File;
    }

    public uint? Crc32
    {
        get;
        set
        {
            SetField(ref field, value);

            if (value is null)
            {
                Crc32Hex = null;
                return;
            }

            Crc32Hex = Crc32ToHex((uint)value);
        }
    }

    private static string Crc32ToHex(uint crc32) => crc32.ToString("X8");

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
                newPath = LocalFilesMixin.RenameFile(oldPath, newName, Logger);
                newPath = BrowserPathRules.EnsureFile(
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
                newPath = BrowserPathRules.CombineFile(
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
