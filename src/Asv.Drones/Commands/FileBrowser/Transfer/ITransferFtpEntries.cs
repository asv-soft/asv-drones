using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Mavlink;

namespace Asv.Drones;

public interface ITransferFtpEntries : IRoutable
{
    ValueTask UploadItem(
        string source,
        string destination,
        FtpEntryType type,
        CancellationToken ct
    );
    ValueTask DownloadItem(
        string source,
        string destination,
        byte partSize,
        FtpEntryType type,
        CancellationToken ct
    );
    ValueTask BurstDownloadItem(
        string source,
        string destination,
        byte partSize,
        FtpEntryType type,
        CancellationToken ct
    );
}
