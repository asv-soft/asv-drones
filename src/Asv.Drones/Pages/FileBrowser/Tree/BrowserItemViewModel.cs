using System.Linq;
using System.Text.RegularExpressions;
using Asv.Avalonia;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public class BrowserItemViewModel : HeadlinedViewModel, IBrowserItemViewModel
{
    public BrowserItemViewModel(
        NavigationId id,
        string? parentPath,
        string path,
        EntityType type,
        ILoggerFactory loggerFactory
    )
        : base(id, loggerFactory)
    {
        ParentPath = parentPath;
        Path = path;
        Type = type;
        Order = 0;

        EditedName = new BindableReactiveProperty<string>();
    }

    public new string? Header
    {
        get => BrowserNamingPolicy.SanitizeForDisplay(field);
        set => SetField(ref field, value);
    } = string.Empty;

    public string Path
    {
        get;
        set => SetField(ref field, value);
    }

    public string? ParentPath
    {
        get;
        set => SetField(ref field, value);
    }

    public FileSize? Size
    {
        get;
        set => SetField(ref field, value);
    }

    public bool HasChildren
    {
        get;
        set => SetField(ref field, value);
    }

    public bool IsExpanded
    {
        get;
        set => SetField(ref field, value);
    }

    public bool IsSelected
    {
        get;
        set => SetField(ref field, value);
    }

    public EntityType Type
    {
        get;
        set => SetField(ref field, value);
    }

    public bool EditMode
    {
        get;
        set => SetField(ref field, value);
    }

    public BindableReactiveProperty<string> EditedName { get; set; }

    public string? Crc32Hex
    {
        get;
        protected set => SetField(ref field, value);
    }

    public Crc32Status Crc32Status
    {
        get;
        set => SetField(ref field, value);
    } = Crc32Status.Default;

    public FtpEntryType FtpEntryType
    {
        get;
        set => SetField(ref field, value);
    }
}
