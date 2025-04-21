using Asv.Avalonia;
using Asv.Mavlink;
using Avalonia.Media;

namespace Asv.Drones;

public class BrowserItem : HeadlinedViewModel, IBrowserItem
{
    private string _path = string.Empty;
    private string? _parentPath;
    private FileSize? _size;
    private bool _hasChildren;
    private bool _isExpanded;
    private bool _isSelected;
    private bool _isInEditMode;
    private string _editedName = string.Empty;
    private string? _crc32Hex;
    private SolidColorBrush _crc32Color = null!;
    private FtpEntryType _ftpEntryType;

    public BrowserItem(NavigationId id, string? parentPath, string path)
        : base(id)
    {
        ParentPath = parentPath;
        Path = path;
        Order = 0;
    }

    public string Path
    {
        get => _path;
        set => SetField(ref _path, value);
    }

    public string? ParentPath
    {
        get => _parentPath;
        set => SetField(ref _parentPath, value);
    }

    public FileSize? Size
    {
        get => _size;
        set => SetField(ref _size, value);
    }

    public bool HasChildren
    {
        get => _hasChildren;
        set => SetField(ref _hasChildren, value);
    }

    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetField(ref _isExpanded, value);
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetField(ref _isSelected, value);
    }

    public bool IsInEditMode
    {
        get => _isInEditMode;
        set => SetField(ref _isInEditMode, value);
    }

    public string EditedName
    {
        get => _editedName;
        set => SetField(ref _editedName, value);
    }

    public string? Crc32Hex
    {
        get => _crc32Hex;
        set => SetField(ref _crc32Hex, value);
    }

    public SolidColorBrush Crc32Color
    {
        get => _crc32Color;
        set => SetField(ref _crc32Color, value);
    }

    public FtpEntryType FtpEntryType
    {
        get => _ftpEntryType;
        set => SetField(ref _ftpEntryType, value);
    }
}
