using Asv.Avalonia;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;

namespace Asv.Drones;

public class FileItemViewModel : BrowserItemViewModel
{
    private uint? _crc32;

    public FileItemViewModel(
        NavigationId id,
        string parentPath,
        string path,
        string? name,
        long size, 
        ILoggerFactory loggerFactory
    )
        : base(id, parentPath, path, loggerFactory)
    {
        HasChildren = false;
        Header = name;
        Size = new FileSize(size);
        FtpEntryType = FtpEntryType.File;
    }

    public uint? Crc32
    {
        get => _crc32;
        set
        {
            SetField(ref _crc32, value);

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
