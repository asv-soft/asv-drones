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
    public FileItemViewModel(
        NavigationId id,
        string parentPath,
        string path,
        string name,
        long size,
        FtpBrowserSourceType type,
        ILoggerFactory loggerFactory
    )
        : base(id, parentPath, path, type, loggerFactory)
    {
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

        var result = await Backend.UseAsync(
            Type,
            onLocal: local =>
            {
                var newFilePath = local.RenameFile(oldPath, newPath, Logger);
                return ValueTask.FromResult(newFilePath);
            },
            onRemote: async ftp =>
            {
                var newFilePath = await ftp.RenameAsync(oldPath, newPath, ct);
                return newFilePath;
            }
        );
        EditMode = false;
        EditedName.Value = FtpBrowserPath.NameOf(result, sep);
        IsSelected = true;
        return newPath;
    }
}
