using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asv.Common;
using Microsoft.Extensions.Logging;

namespace Asv.Drones;

public static class LocalFilesMixin
{
    public static IReadOnlyList<IBrowserItemViewModel> LoadBrowserItems(
        string path,
        string root,
        ILoggerFactory loggerFactory,
        CancellationToken ct = default,
        ILogger? log = null
    )
    {
        var result = new ConcurrentBag<IBrowserItemViewModel>();
        ProcessBrowserDirectory(path, root, result,loggerFactory, ct, log);
        log?.LogTrace("Directory processed ({Path})", path);
        return result.ToList();
    }

    private static void ProcessBrowserDirectory(
        string path,
        string root,
        ConcurrentBag<IBrowserItemViewModel> items,
        ILoggerFactory loggerFactory,
        CancellationToken ct = default,
        ILogger? log = null
    )
    {
        ct.ThrowIfCancellationRequested();

        foreach (var dir in Directory.EnumerateDirectories(path))
        {
            ct.ThrowIfCancellationRequested();
            var info = new DirectoryInfo(dir);

            var id = PathHelper.EncodePathToId(dir);
            var parent = info.Parent?.FullName ?? root;

            items.Add(new DirectoryItemViewModel(id, parent, dir, info.Name, loggerFactory));
            ProcessBrowserDirectory(dir, root, items, loggerFactory, ct);
        }

        foreach (var file in Directory.EnumerateFiles(path))
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                var info = new FileInfo(file);
                var id = PathHelper.EncodePathToId(file);
                var parent = info.Directory?.FullName ?? root;
                items.Add(new FileItemViewModel(id, parent, file, info.Name, info.Length, loggerFactory));
            }
            catch (FileNotFoundException ex)
            {
                log?.LogWarning(ex, "Skipped file '{File}' during scanning", file);
            }
        }
    }

    public static string RenameFile(string oldPath, string newName, ILogger? log = null)
    {
        var parentDir = Path.GetDirectoryName(oldPath)!;
        var newPath = Path.Combine(parentDir, newName);
        try
        {
            if (File.Exists(newPath))
            {
                var baseName = Path.GetFileNameWithoutExtension(newName);
                var ext = Path.GetExtension(newName);
                var counter = 1;
                while (File.Exists(newPath))
                {
                    newPath = Path.Combine(parentDir, $"{baseName} ({counter++}){ext}");
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

        File.Move(oldPath, newPath);

        log?.LogInformation("File renamed to '{new}'", newPath);

        return newPath;
    }

    public static string RenameDirectory(string oldPath, string newName, ILogger? log = null)
    {
        var parentDir = Path.GetDirectoryName(oldPath)!;
        var newPath = Path.Combine(parentDir, newName);

        try
        {
            if (Directory.Exists(newPath))
            {
                var counter = 1;
                while (Directory.Exists(newPath))
                {
                    newPath = Path.Combine(parentDir, $"{newName} ({counter++})");
                }
            }
            else
            {
                throw new FileNotFoundException("Path not found");
            }
        }
        catch (FileNotFoundException e)
        {
            log?.LogError(e, "Failed to rename directory. Incorrect path: {Path}", oldPath);
        }

        Directory.Move(oldPath, newPath);

        log?.LogInformation("Directory renamed to '{new}'", newPath);

        return newPath;
    }

    public static DirectoryInfo CreateDirectory(string path, ILogger? log = null)
    {
        var folderNumber = 1;
        while (true)
        {
            var name = Path.Combine(path, $"Folder{folderNumber}");
            if (Directory.Exists(name))
            {
                folderNumber++;
                continue;
            }

            log?.LogInformation("Creating directory '{Path}'", name);
            return Directory.CreateDirectory(name);
        }
    }

    public static void RemoveFile(string path, ILogger? log = null)
    {
        try
        {
            File.Delete(path);
            log?.LogInformation("File removed: '{Path}'", path);
        }
        catch (FileNotFoundException e)
        {
            log?.LogError(e, "File '{Path}' not found", path);
        }
    }

    public static void RemoveDirectory(string path, bool recursive, ILogger? log = null)
    {
        try
        {
            Directory.Delete(path, recursive);
            log?.LogInformation("Directory removed: '{Path}'", path);
        }
        catch (FileNotFoundException e)
        {
            log?.LogError(e, "Directory '{Path}' not found", path);
        }
    }

    public static async Task WriteFileAsync(
        string directoryPath,
        string fileName,
        byte[] data,
        CancellationToken ct = default,
        ILogger? log = null
    )
    {
        try
        {
            var fullPath = Path.Combine(directoryPath, fileName);

            await File.WriteAllBytesAsync(fullPath, data, ct);
            log?.LogInformation("File created successfully: {name}", fileName);
        }
        catch (Exception e)
        {
            log?.LogError(e, "Failed to create file '{Name}'", fileName);
        }
    }

    public static async Task<uint> CalculateCrc32Async(
        string path,
        CancellationToken ct = default,
        ILogger? log = null
    )
    {
        try
        {
            var crc32 = Crc32Mavlink.Accumulate(await File.ReadAllBytesAsync(path, ct));
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
