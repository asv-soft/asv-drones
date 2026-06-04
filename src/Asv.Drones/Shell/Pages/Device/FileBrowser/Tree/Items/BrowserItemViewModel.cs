using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Common;
using Asv.Mavlink;
using Asv.Modeling;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones;

public abstract class BrowserItemViewModel : ViewModel, IBrowserItemViewModel
{
    private readonly char _separator;
    private IBrowserItemsOperations? _ops;
    private readonly ILogger<BrowserItemViewModel> _logger;

    protected IBrowserItemsOperations ItemsOperations =>
        _ops
        ?? throw new InvalidOperationException(
            $"{nameof(IBrowserItemsOperations)} are not attached. Call {nameof(AttachBackend)} first."
        );

    protected BrowserItemViewModel(
        string id,
        string? parentPath,
        string path,
        FtpBrowserSourceType type,
        ILoggerFactory loggerFactory
    )
        : base(id)
    {
        ParentPath = parentPath;
        Path = path;
        Type = type;
        _logger = loggerFactory.CreateLogger<BrowserItemViewModel>();
        _separator =
            Type is FtpBrowserSourceType.Local
                ? System.IO.Path.DirectorySeparatorChar
                : MavlinkFtpHelper.DirectorySeparator;

        EditedName = new BindableReactiveProperty<string>().DisposeItWith(Disposable);
        EditedName
            .EnableValidation(x =>
            {
                var result = FtpBrowserNamingPolicy.Validate(x);
                return !result.IsSuccess ? result.ValidationException : null;
            })
            .ForceValidate();
        CommitRename = CanRename
            .ToReactiveCommand<Unit>(
                async (_, ct) => await CommitRenameImpl(ct),
                awaitOperation: AwaitOperation.Drop
            )
            .DisposeItWith(Disposable);
    }

    #region Properties

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
        protected init => SetField(ref field, value);
    }

    public BindableReactiveProperty<string> EditedName { get; set; }

    #endregion

    public Observable<bool> CanRename =>
        EditedName.Select(_ => !EditedName.HasErrors).DistinctUntilChanged().Share();

    #region Commands

    public ReactiveCommand<Unit> CommitRename { get; }

    #endregion

    public abstract ValueTask<string> RenameAsync(
        string oldValue,
        string newValue,
        CancellationToken ct
    );

    public abstract ValueTask RemoveAsync(CancellationToken ct);

    public abstract ValueTask<uint> CalculateCrc32Async(CancellationToken ct);
    public abstract ValueTask CreateDirectoryAsync(CancellationToken ct);

    /// <summary>
    /// Attach backend context once right after VM creation.
    /// </summary>
    /// <param name="backend">Backend context from VM.</param>
    public void AttachBackend(FileBrowserBackend backend)
    {
        _ops = backend.ResolveOps(Type);
    }

    public ValueTask Remove(CancellationToken ct)
    {
        return ValueTask.CompletedTask;
    }

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

        string resultPath;
        try
        {
            resultPath = await RenameAsync(oldPath, newPath, ct);
        }
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "Failed to rename \"{OldPath}\" to \"{NewPath}\"",
                oldPath,
                newPath
            );
            EditedName.Value = oldName;
            return;
        }

        await this.Rise(new BrowserItemRenamedEvent(this, oldPath, resultPath, Type), ct);
    }
}
