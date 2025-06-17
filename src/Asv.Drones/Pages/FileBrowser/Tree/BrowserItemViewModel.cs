using System.Linq;
using Asv.Avalonia;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;

namespace Asv.Drones;

public class BrowserItemViewModel : HeadlinedViewModel, IBrowserItemViewModel
{
    private string? _headerRaw = string.Empty;
    private string _path = string.Empty;
    private string? _parentPath;
    private FileSize? _size;
    private bool _hasChildren;
    private bool _isExpanded;
    private bool _isSelected;
    private bool _isInEditMode;
    private string _editedName = string.Empty;
    private string? _crc32Hex;
    private Crc32Status _crc32Status = Crc32Status.Default;
    private FtpEntryType _ftpEntryType;

    public BrowserItemViewModel(NavigationId id, string? parentPath, string path, ILoggerFactory loggerFactory)
        : base(id, loggerFactory)
    {
        ParentPath = parentPath;
        Path = path;
        Order = 0;
    }

    public new string? Header
    {
        get =>
            _headerRaw == null
                ? null
                : new string(
                    _headerRaw.Select(ch => ch is >= (char)32 and <= (char)126 ? ch : '*').ToArray()
                );
        set => SetField(ref _headerRaw, value);
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
        protected set => SetField(ref _crc32Hex, value);
    }

    public Crc32Status Crc32Status
    {
        get => _crc32Status;
        set => SetField(ref _crc32Status, value);
    }

    public FtpEntryType FtpEntryType
    {
        get => _ftpEntryType;
        set => SetField(ref _ftpEntryType, value);
    }
}
