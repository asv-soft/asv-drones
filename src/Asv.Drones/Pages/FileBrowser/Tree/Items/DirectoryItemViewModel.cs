using System;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;

namespace Asv.Drones;

public class DirectoryItemViewModel : BrowserItemViewModel, ISupportRename
{
    private readonly FtpClientService? _ftpService;

    public DirectoryItemViewModel(
        NavigationId id,
        string? parentPath,
        string path,
        string name,
        FtpBrowserSourceType type,
        FtpClientService? ftpService,
        ILoggerFactory loggerFactory
    )
        : base(id, parentPath, path, type, loggerFactory)
    {
        _ftpService = ftpService;
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

        switch (Type)
        {
            case FtpBrowserSourceType.Local:
            {
                var result = LocalFilesMixin.RenameDirectory(oldPath, newPath, Logger);
                result = FtpBrowserPath.Normalize(result, true, sep);

                EditMode = false;
                EditedName.Value = FtpBrowserPath.NameOf(result, sep);
                IsSelected = true;
                return result;
            }
            case FtpBrowserSourceType.Remote:
            {
                if (_ftpService is null)
                {
                    throw new InvalidOperationException("FTP service is not initialized");
                }

                await _ftpService.RenameAsync(oldPath, newPath, ct).ConfigureAwait(false);

                EditMode = false;
                EditedName.Value = FtpBrowserPath.NameOf(newPath, sep);
                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }
        IsSelected = true;
        return newPath;
    }
}
