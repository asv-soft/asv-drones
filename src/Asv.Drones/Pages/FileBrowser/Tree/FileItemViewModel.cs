using System;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;
using R3;

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
        FtpClientService? ftpService,
        ILoggerFactory loggerFactory
    )
        : base(id, parentPath, path, type, ftpService, loggerFactory)
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
}
