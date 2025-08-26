using System;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public class FileItemViewModel : BrowserItemViewModel, ISupportRename
{
    private readonly FtpClientService? _ftpService;

    public FileItemViewModel(
        NavigationId id,
        string parentPath,
        string path,
        string name,
        long size,
        FtpBrowserSourceType type,
        FtpClientService? ftpService,
        ILoggerFactory loggerFactory
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

        var oldPath = FtpBrowserPath.Normalize(oldValue, false, sep);
        var newPath = FtpBrowserPath.Normalize(newValue, false, sep);

        switch (Type)
        {
            case FtpBrowserSourceType.Local:
            {
                var result = LocalFilesMixin.RenameFile(oldPath, newPath, Logger);
                result = FtpBrowserPath.Normalize(result, false, sep);

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
