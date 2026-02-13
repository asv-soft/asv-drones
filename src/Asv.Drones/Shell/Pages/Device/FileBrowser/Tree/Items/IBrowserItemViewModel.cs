using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Drones.Api;
using Asv.Mavlink;
using R3;

namespace Asv.Drones;

public interface IBrowserItemViewModel : ISupportRename, ISupportRemove
{
    string Name { get; set; }
    string Path { get; set; }
    string? ParentPath { get; set; }
    FileSize? Size { get; }
    bool HasChildren { get; }
    bool IsExpanded { get; set; }
    bool IsSelected { get; set; }
    bool EditMode { get; set; }
    FtpBrowserSourceType Type { get; }
    string? Crc32Hex { get; }
    Crc32Status Crc32Status { get; }
    FtpEntryType FtpEntryType { get; }
    ReactiveCommand<Unit> CommitRename { get; }
    BindableReactiveProperty<string> EditedName { get; set; }

    void AttachBackend(FileBrowserBackend backend);

    ValueTask CreateDirectoryAsync(CancellationToken ct);
    ValueTask<uint> CalculateCrc32Async(CancellationToken ct);
}
