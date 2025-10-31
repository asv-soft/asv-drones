using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Common;
using Microsoft.Extensions.Logging;

namespace Asv.Drones;

public class LocalFilesService(IFileSystem? fileSystem = null)
{
    private readonly IFileSystem _fileSystem = fileSystem ?? new FileSystem();

    public IReadOnlyList<IBrowserItemViewModel> LoadBrowserItems(
        string path,
        string root,
        FileBrowserBackend backend,
        ILoggerFactory loggerFactory,
        IDictionary<string, DirectoryItemViewModelConfig>? directoryCfgs,
        CancellationToken ct = default,
        ILogger? log = null
    )
    {
        var result = new ConcurrentBag<IBrowserItemViewModel>();
        ProcessBrowserDirectory(
            path,
            root,
            ref result,
            backend,
            loggerFactory,
            directoryCfgs,
            ct,
            log
        );
        log?.LogTrace("Directory processed ({Path})", path);
        return result.ToList();
    }

    private void ProcessBrowserDirectory(
        string path,
        string root,
        ref ConcurrentBag<IBrowserItemViewModel> items,
        FileBrowserBackend backend,
        ILoggerFactory loggerFactory,
        IDictionary<string, DirectoryItemViewModelConfig>? directoryCfgs,
        CancellationToken ct = default,
        ILogger? log = null
    )
    {
        ct.ThrowIfCancellationRequested();

        var rootInfo = new DirectoryInfo(root);
        var rootId = PathHelper.EncodePathToId(root);

        var rootVm = new DirectoryItemViewModel(
            rootId,
            null,
            root,
            rootInfo.Name,
            FtpBrowserSourceType.Local,
            loggerFactory
        );

        rootVm.AttachBackend(backend);
        items.Add(rootVm);

        foreach (var dir in _fileSystem.Directory.EnumerateDirectories(path))
        {
            ct.ThrowIfCancellationRequested();
            var info = new DirectoryInfo(dir);

            var id = PathHelper.EncodePathToId(dir);
            var parent = info.Parent?.FullName ?? root;

            DirectoryItemViewModelConfig? cfg = null;
            directoryCfgs?.TryGetValue(dir, out cfg);
            var vm = new DirectoryItemViewModel(
                id,
                parent,
                dir,
                info.Name,
                FtpBrowserSourceType.Local,
                loggerFactory,
                cfg
            );

            vm.AttachBackend(backend);

            items.Add(vm);
            ProcessBrowserDirectory(
                dir,
                root,
                ref items,
                backend,
                loggerFactory,
                directoryCfgs,
                ct
            );
        }

        foreach (var file in _fileSystem.Directory.EnumerateFiles(path))
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                var info = new FileInfo(file);
                var id = PathHelper.EncodePathToId(file);
                var parent = info.Directory?.FullName ?? root;
                var vm = new FileItemViewModel(
                    id,
                    parent,
                    file,
                    info.Name,
                    info.Length,
                    FtpBrowserSourceType.Local,
                    loggerFactory
                );
                vm.AttachBackend(backend);
                items.Add(vm);
            }
            catch (FileNotFoundException ex)
            {
                log?.LogWarning(ex, "Skipped file '{File}' during scanning", file);
            }
        }
    }

    public string RenameFile(string oldPath, string newPath, ILogger? log = null)
    {
        try
        {
            if (_fileSystem.File.Exists(newPath))
            {
                var parentDir = _fileSystem.Path.GetDirectoryName(oldPath) ?? string.Empty;
                var baseName = _fileSystem.Path.GetFileNameWithoutExtension(newPath);
                var ext = _fileSystem.Path.GetExtension(newPath);
                var counter = 1;
                while (_fileSystem.File.Exists(newPath))
                {
                    newPath = _fileSystem.Path.Combine(parentDir, $"{baseName} ({counter++}){ext}");
                }
            }
            _fileSystem.File.Move(oldPath, newPath);
            log?.LogInformation("File renamed to '{new}'", newPath);
        }
        catch (FileNotFoundException e)
        {
            log?.LogError(e, "Failed to rename file. Incorrect path: {Path}", oldPath);
        }

        return newPath;
    }

    public string RenameDirectory(string oldPath, string newPath, ILogger? log = null)
    {
        try
        {
            if (_fileSystem.Directory.Exists(newPath))
            {
                var parentDir = _fileSystem.Path.GetDirectoryName(oldPath) ?? string.Empty;
                var baseName = _fileSystem.Path.GetFileNameWithoutExtension(newPath);
                var counter = 1;
                while (_fileSystem.Directory.Exists(newPath))
                {
                    newPath = _fileSystem.Path.Combine(parentDir, $"{baseName} ({counter++})");
                }
            }
            _fileSystem.Directory.Move(oldPath, newPath);
            log?.LogInformation("Directory renamed to '{new}'", newPath);
        }
        catch (DirectoryNotFoundException e)
        {
            log?.LogError(e, "Failed to rename directory. Incorrect path: {Path}", oldPath);
        }

        return newPath;
    }

    public void CreateDirectory(string path, ILogger? log = null)
    {
        var folderNumber = 1;
        while (true)
        {
            var name = _fileSystem.Path.Combine(path, $"Folder{folderNumber}");
            if (_fileSystem.Directory.Exists(name))
            {
                folderNumber++;
                continue;
            }

            log?.LogInformation("Creating directory '{Path}'", name);
            _fileSystem.Directory.CreateDirectory(name);
            return;
        }
    }

    public void RemoveFile(string path, ILogger? log = null)
    {
        try
        {
            _fileSystem.File.Delete(path);
            log?.LogInformation("File removed: '{Path}'", path);
        }
        catch (FileNotFoundException e)
        {
            log?.LogError(e, "File '{Path}' not found", path);
        }
    }

    public void RemoveDirectory(string path, bool recursive, ILogger? log = null)
    {
        try
        {
            _fileSystem.Directory.Delete(path, recursive);
            log?.LogInformation("Directory removed: '{Path}'", path);
        }
        catch (FileNotFoundException e)
        {
            log?.LogError(e, "Directory '{Path}' not found", path);
        }
    }

    public async Task<uint> CalculateCrc32Async(
        string path,
        CancellationToken ct = default,
        ILogger? log = null
    )
    {
        try
        {
            var crc32 = Crc32Mavlink.Accumulate(await _fileSystem.File.ReadAllBytesAsync(path, ct));
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
