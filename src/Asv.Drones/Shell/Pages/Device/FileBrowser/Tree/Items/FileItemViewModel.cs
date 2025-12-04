using System;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;

namespace Asv.Drones;

public class FileItemViewModel : BrowserItemViewModel
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

    public override async ValueTask<string> RenameAsync(
        string oldValue,
        string newValue,
        CancellationToken ct
    )
    {
        ArgumentException.ThrowIfNullOrEmpty(oldValue);
        ArgumentException.ThrowIfNullOrEmpty(newValue);

        var sep = ItemsOperations.Separator;

        var oldPath = FtpBrowserPath.Normalize(oldValue, false, sep);
        var newPath = FtpBrowserPath.Normalize(newValue, false, sep);

        var result = await ItemsOperations.RenameFileAsync(oldPath, newPath, Logger, ct);

        EditMode = false;
        EditedName.Value = FtpBrowserPath.NameOf(result, sep);
        IsSelected = true;
        return newPath;
    }

    public override async ValueTask RemoveAsync(CancellationToken ct)
    {
        await ItemsOperations.RemoveFileAsync(Path, Logger, ct);
    }

    public override async ValueTask<uint> CalculateCrc32Async(CancellationToken ct)
    {
        var crc32 = await ItemsOperations.CalculateCrc32Async(Path, Logger, ct);
        Crc32 = crc32;
        Crc32Status = Crc32Status.Default;
        return crc32;
    }

    public override async ValueTask CreateDirectoryAsync(CancellationToken ct)
    {
        var path = FtpBrowserPath.ParentDirOf(Path, ItemsOperations.Separator);
        await ItemsOperations.CreateDirectoryAsync(path, Logger, ct);
    }
}
