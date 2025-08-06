using Asv.Avalonia;
using Asv.Mavlink;
using R3;

namespace Asv.Drones;

public interface IBrowserItemViewModel : IHeadlinedViewModel
{
    string Path { get; }
    string? ParentPath { get; }
    FileSize? Size { get; }
    bool HasChildren { get; }
    bool IsExpanded { get; }
    bool IsSelected { get; }
    EntityType Type { get; }
    bool EditMode { get; }
    BindableReactiveProperty<string> EditedName { get; }
    string? Crc32Hex { get; }
    Crc32Status Crc32Status { get; }
    FtpEntryType FtpEntryType { get; }
}
