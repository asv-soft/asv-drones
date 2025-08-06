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
        string? name,
        long size,
        EntityType type,
        ILoggerFactory loggerFactory
    )
        : base(id, parentPath, path, type, loggerFactory)
    {
        HasChildren = false;
        Header = name;
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
}
