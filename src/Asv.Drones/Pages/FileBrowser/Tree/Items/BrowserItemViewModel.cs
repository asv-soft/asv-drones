using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Common;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public class BrowserItemViewModel : RoutableViewModel, IBrowserItemViewModel
{
    private readonly char _separator;

    public BrowserItemViewModel(
        NavigationId id,
        string? parentPath,
        string path,
        FtpBrowserSourceType type,
        ILoggerFactory loggerFactory
    )
        : base(id, loggerFactory)
    {
        ParentPath = parentPath;
        Path = path;
        Type = type;

        _separator =
            Type is FtpBrowserSourceType.Local
                ? System.IO.Path.DirectorySeparatorChar
                : MavlinkFtpHelper.DirectorySeparator;

        EditedName = new BindableReactiveProperty<string>().DisposeItWith(Disposable);
        EditedName
            .EnableValidationRoutable(FtpBrowserNamingPolicy.Validate, this, true)
            .DisposeItWith(Disposable);

        CommitRename = new ReactiveCommand(
            (_, ct) => CommitRenameImpl(ct),
            AwaitOperation.Drop
        ).DisposeItWith(Disposable);
    }

    public string Name
    {
        get => FtpBrowserNamingPolicy.SanitizeForDisplay(field);
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

    public FtpBrowserSourceType Type
    {
        get;
        set => SetField(ref field, value);
    }

    public bool EditMode
    {
        get;
        set => SetField(ref field, value);
    }

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

    public ReactiveCommand CommitRename { get; }

    public BindableReactiveProperty<string> EditedName { get; set; }

    private async ValueTask CommitRenameImpl(CancellationToken ct)
    {
        var oldName = Name;
        var oldPath = Path;
        var newName = string.IsNullOrWhiteSpace(EditedName.Value)
            ? FtpBrowserNamingPolicy.BlankName
            : EditedName.Value;
        var parentDir = FtpBrowserPath.ParentDirOf(oldPath, _separator);
        var isDirectory = FtpEntryType == FtpEntryType.Directory;
        var newPath = isDirectory
            ? FtpBrowserPath.CombineDir(parentDir, newName, _separator)
            : FtpBrowserPath.CombineFile(parentDir, newName, _separator);

        EditMode = false;
        if (
            string.IsNullOrEmpty(newPath)
            || string.Equals(newName, oldName, StringComparison.Ordinal)
        )
        {
            EditedName.Value = oldName;
            return;
        }
        await this.ExecuteCommand(
            CommitRenameCommand.Id,
            CommandArg.CreateList(
                CommandArg.CreateString(oldPath),
                CommandArg.CreateString(newPath)
            ),
            ct
        );
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }
}
