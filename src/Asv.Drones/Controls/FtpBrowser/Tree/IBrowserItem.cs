using Asv.Avalonia;
using Asv.Mavlink;
using Avalonia.Media;

namespace Asv.Drones;

public interface IBrowserItem : IHeadlinedViewModel
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
    SolidColorBrush Crc32Color { get; }
    FtpEntryType FtpEntryType { get; }
}
