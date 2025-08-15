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

        EditedName = new BindableReactiveProperty<string>().DisposeItWith(Disposable);
        EditedName
            .EnableValidationRoutable(BrowserNamingPolicy.Validate, this, true)
            .DisposeItWith(Disposable);

        CommitRename = new ReactiveCommand(
            (_, ct) => CommitRenameImpl(ct),
            AwaitOperation.Drop
        ).DisposeItWith(Disposable);
    }

    public string Name
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

    public ReactiveCommand CommitRename { get; }

    private async ValueTask CommitRenameImpl(CancellationToken ct)
    {
        var oldName = Name;
        var oldPath = Path;
        var newName = string.IsNullOrWhiteSpace(EditedName.Value)
            ? BrowserNamingPolicy.BlankName
            : EditedName.Value;

        EditMode = false;
        if (string.Equals(newName, oldName, StringComparison.Ordinal))
        {
            EditedName.Value = oldName;
            return;
        }

        try
        {
            await this.ExecuteCommand(
                RenameItemCommand.Id,
                CommandArg.ChangeAction(oldPath, CommandArg.CreateString(newName)),
                ct
            );
        }
        catch (Exception ex)
        {
            Logger.LogError(
                ex,
                "Failed to rename remote item {oldName} to {newName}",
                oldName,
                newName
            );
            Name = oldName;
            EditedName.Value = oldName;
        }
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }
}
