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

public class BrowserItemViewModel : RoutableViewModel, IBrowserItemViewModel, ISupportRename
{
    private readonly FtpClientService? _ftpService;
    private readonly char _separator;

    public BrowserItemViewModel(
        NavigationId id,
        string? parentPath,
        string path,
        EntityType type,
        FtpClientService? ftpService,
        ILoggerFactory loggerFactory
    )
        : base(id, loggerFactory)
    {
        _ftpService = ftpService;
        ParentPath = parentPath;
        Path = path;
        Type = type;

        _separator =
            Type is EntityType.Local
                ? System.IO.Path.DirectorySeparatorChar
                : MavlinkFtpHelper.DirectorySeparator;

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
        var parentDir = BrowserPathRules.ParentDirOf(oldPath, _separator);
        var isDirectory = FtpEntryType == FtpEntryType.Directory;
        var newPath = isDirectory
            ? BrowserPathRules.CombineDir(parentDir, newName, _separator)
            : BrowserPathRules.CombineFile(parentDir, newName, _separator);

        EditMode = false;
        if (
            string.IsNullOrEmpty(newPath)
            || string.Equals(newName, oldName, StringComparison.Ordinal)
        )
        {
            EditedName.Value = oldName;
            return;
        }

        try
        {
            await this.ExecuteCommand(
                CommitRenameCommand.Id,
                CommandArg.CreateList(
                    CommandArg.CreateString(oldPath),
                    CommandArg.CreateString(newPath)
                ),
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

    public async ValueTask<string> RenameAsync(
        string oldValue,
        string newValue,
        CancellationToken ct
    )
    {
        if (string.IsNullOrWhiteSpace(oldValue))
        {
            throw new ArgumentException("Old path is empty", nameof(oldValue));
        }

        if (string.IsNullOrWhiteSpace(newValue))
        {
            throw new ArgumentException("New name is empty", nameof(newValue));
        }

        var isDir = FtpEntryType == FtpEntryType.Directory;
        var sep =
            Type == EntityType.Remote
                ? MavlinkFtpHelper.DirectorySeparator
                : System.IO.Path.DirectorySeparatorChar;

        var oldPath = BrowserPathRules.Normalize(oldValue, isDir, sep);

        var newPath = BrowserPathRules.Normalize(newValue, isDir, sep);

        switch (Type)
        {
            case EntityType.Local when !isDir:
            {
                var result = LocalFilesMixin.RenameFile(oldPath, newPath, Logger);
                result = BrowserPathRules.Normalize(result, isDir, sep);

                EditMode = false;
                EditedName.Value = BrowserPathRules.NameOf(result, sep);
                IsSelected = true;
                return result;
            }
            case EntityType.Local when isDir:
            {
                var result = LocalFilesMixin.RenameDirectory(oldPath, newPath, Logger);
                result = BrowserPathRules.Normalize(result, isDir, sep);

                EditMode = false;
                EditedName.Value = BrowserPathRules.NameOf(result, sep);
                IsSelected = true;
                return result;
            }
            case EntityType.Remote when !isDir:
            case EntityType.Remote when isDir:
            {
                if (_ftpService is null)
                {
                    throw new InvalidOperationException("FTP service is not initialized");
                }

                await _ftpService.RenameAsync(oldPath, newPath, ct).ConfigureAwait(false);

                EditMode = false;
                EditedName.Value = BrowserPathRules.NameOf(newPath, sep);
                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }
        IsSelected = true;
        return newPath;
    }
}
