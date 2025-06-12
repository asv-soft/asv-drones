using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asv.Common;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;
using ObservableCollections;

namespace Asv.Drones;

public static class BrowserFtpExtension
{
    public static IReadOnlyObservableList<IBrowserItemViewModel> CopyEntriesAsBrowserItems(
        this IFtpClientEx clientEx
    )
    {
        // TODO: need to work with this collection as with an observable collection
        var items = new ObservableList<IBrowserItemViewModel>();
        clientEx.Entries.ForEach(e =>
        {
            if (e.Value.Path == MavlinkFtpHelper.DirectorySeparator.ToString())
            {
                var root = new DirectoryItemViewModel(
                    "_",
                    string.Empty,
                    MavlinkFtpHelper.DirectorySeparator.ToString(),
                    "_"
                );

                items.Add(root);
            }

            var item = e.Value.Type switch
            {
                FtpEntryType.Directory => new DirectoryItemViewModel(
                    PathHelper.EncodePathToId(e.Value.Path),
                    e.Value.ParentPath,
                    e.Key,
                    e.Value.Name
                ),
                FtpEntryType.File => new FileItemViewModel(
                    PathHelper.EncodePathToId(e.Value.Path),
                    e.Value.ParentPath,
                    e.Key,
                    e.Value.Name,
                    ((FtpFile)e.Value).Size
                ),
                _ => new BrowserItemViewModel(
                    PathHelper.EncodePathToId(e.Value.Path),
                    e.Value.ParentPath,
                    e.Key
                ),
            };

            items.Add(item);
        });
        return items;
    }

    public static async Task<string> RenameAsync(
        this IFtpClientEx clientEx,
        string oldPath,
        string newName,
        CancellationToken ct = default,
        ILogger? log = null
    )
    {
        var parentDir = clientEx.Entries[oldPath].ParentPath;
        var newPath = MavlinkFtpHelper.Combine(parentDir, newName);

        try
        {
            if (
                clientEx.Entries.ContainsKey(newPath)
                || clientEx.Entries.ContainsKey(newPath + MavlinkFtpHelper.DirectorySeparator)
            )
            {
                var baseName = Path.GetFileNameWithoutExtension(newName);
                var ext = Path.GetExtension(newName);
                var counter = 1;

                while (
                    clientEx.Entries.ContainsKey(newPath)
                    || clientEx.Entries.ContainsKey(newPath + MavlinkFtpHelper.DirectorySeparator)
                )
                {
                    newPath = MavlinkFtpHelper.Combine(parentDir, $"{baseName} ({counter++}){ext}");
                }
            }
            else
            {
                throw new FileNotFoundException("Path not found");
            }
        }
        catch (FileNotFoundException e)
        {
            log?.LogError(e, "Failed to rename file. Incorrect path: {Path}", oldPath);
        }

        log?.LogInformation("File renamed to '{new}'", newPath);

        await clientEx.Base.Rename(oldPath, newPath, ct);

        return newPath;
    }

    public static async Task RemoveDirectoryAsync(
        this IFtpClientEx clientEx,
        string path,
        bool recursive = true,
        CancellationToken ct = default,
        ILogger? log = null
    )
    {
        try
        {
            if (recursive)
            {
                await RemoveDirectoryRecursive(path);
            }
            else
            {
                await clientEx.Base.RemoveDirectory(path, ct);
            }

            log?.LogInformation("Directory removed: {path}", path);
        }
        catch (Exception e)
        {
            log?.LogError(e, "Failed to remove directory");
        }

        return;

        async Task RemoveDirectoryRecursive(string directoryPath)
        {
            var itemsInDir = clientEx
                .Entries.Where(x => x.Value.ParentPath == directoryPath)
                .ToList();

            foreach (var e in itemsInDir)
            {
                switch (e.Value.Type)
                {
                    case FtpEntryType.Directory:
                        await RemoveDirectoryRecursive(e.Key);
                        break;
                    case FtpEntryType.File:
                        await clientEx.Base.RemoveFile(e.Key, ct);
                        break;
                    default:
                        log?.LogError("Unknown FTP entry type: ({type})", e.Value.Type);
                        break;
                }
            }

            await clientEx.Base.RemoveDirectory(directoryPath, ct);
        }
    }

    public static async Task RemoveFileAsync(
        this IFtpClientEx clientEx,
        string path,
        CancellationToken ct = default,
        ILogger? log = null
    )
    {
        await clientEx.Base.RemoveFile(path, ct);
        log?.LogInformation("File removed: {path}", path);
    }

    public static async Task CreateDirectoryAsync(
        this IFtpClientEx clientEx,
        string path,
        CancellationToken ct = default,
        ILogger? log = null
    )
    {
        var folderNumber = 1;
        while (true)
        {
            var name = $"Folder{folderNumber}{MavlinkFtpHelper.DirectorySeparator}";
            clientEx
                .Entries.FirstOrDefault(x => x.Key == $"{path}{name}")
                .Deconstruct(out var k, out _);
            if (!string.IsNullOrEmpty(k))
            {
                folderNumber++;
                continue;
            }

            name =
                path == $"{MavlinkFtpHelper.DirectorySeparator}"
                    ? $"{MavlinkFtpHelper.DirectorySeparator}{name}"
                    : $"{path}{MavlinkFtpHelper.DirectorySeparator}{name}";

            log?.LogInformation("Creating directory '{Path}'", name);
            await clientEx.Base.CreateDirectory(name, ct);
            return;
        }
    }

    public static async Task<uint> CalculateCrc32Async(
        this IFtpClientEx clientEx,
        string path,
        CancellationToken ct = default,
        ILogger? log = null
    )
    {
        try
        {
            var crc32 = await clientEx.Base.CalcFileCrc32(path, ct);
            log?.LogInformation("File crc32: {crc32}", crc32);
            return crc32;
        }
        catch (FileNotFoundException e)
        {
            log?.LogError(e, "File '{Path}' not found", path);
        }

        return 0U;
    }
}
