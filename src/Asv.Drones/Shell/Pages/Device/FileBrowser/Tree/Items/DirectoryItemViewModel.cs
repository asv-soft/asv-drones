using System;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;

namespace Asv.Drones;

public class DirectoryItemViewModelConfig
{
    public bool IsExpanded { get; set; }
}

public class DirectoryItemViewModel : BrowserItemViewModel
{
    public DirectoryItemViewModel(
        NavigationId id,
        string? parentPath,
        string path,
        string name,
        FtpBrowserSourceType type,
        ILoggerFactory loggerFactory,
        DirectoryItemViewModelConfig? layoutConfig = null
    )
        : base(id, parentPath, path, type, loggerFactory)
    {
        HasChildren = true;
        Name = name;
        FtpEntryType = FtpEntryType.Directory;
        IsExpanded = layoutConfig?.IsExpanded ?? IsExpanded;
    }

    public override async ValueTask<string> RenameAsync(
        string oldValue,
        string newValue,
        CancellationToken ct
    )
    {
        ArgumentException.ThrowIfNullOrEmpty(oldValue);
        ArgumentException.ThrowIfNullOrEmpty(newValue);

        var sep = ItemsOperations.Separator;

        var oldPath = FtpBrowserPath.Normalize(oldValue, true, sep);
        var newPath = FtpBrowserPath.Normalize(newValue, true, sep);

        var result = await ItemsOperations.RenameDirectoryAsync(oldPath, newPath, Logger, ct);

        EditMode = false;
        EditedName.Value = FtpBrowserPath.NameOf(result, sep);
        IsSelected = true;
        return result;
    }

    public override async ValueTask RemoveAsync(CancellationToken ct)
    {
        await ItemsOperations.RemoveDirectoryAsync(Path, Logger, ct);
    }

    public override ValueTask<uint> CalculateCrc32Async(CancellationToken ct)
    {
        Logger.LogError("Cannot calculate CRC32 for directory");
        return new ValueTask<uint>(0);
    }

    public override async ValueTask CreateDirectoryAsync(CancellationToken ct)
    {
        await ItemsOperations.CreateDirectoryAsync(Path, Logger, ct);
    }
}
