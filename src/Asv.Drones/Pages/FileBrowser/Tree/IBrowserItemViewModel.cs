using Asv.Avalonia;
using Asv.Mavlink;

namespace Asv.Drones;

public interface IBrowserItemViewModel : IHeadlinedViewModel
{
    string Path { get; }
    string? ParentPath { get; }
    FileSize? Size { get; }
    bool HasChildren { get; }
    bool IsExpanded { get; }
    bool IsSelected { get; }
    bool IsInEditMode { get; }
    string EditedName { get; }
    string? Crc32Hex { get; }
    Crc32Status Crc32Status { get; }
    FtpEntryType FtpEntryType { get; }
}
