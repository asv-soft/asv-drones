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
    bool IsExpanded { get; set; }
    bool IsSelected { get; set; }
    bool EditMode { get; set; }
    BindableReactiveProperty<string> EditedName { get; set; }
    EntityType Type { get; }
    string? Crc32Hex { get; }
    Crc32Status Crc32Status { get; }
    FtpEntryType FtpEntryType { get; }
}
