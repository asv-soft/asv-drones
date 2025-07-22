using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

namespace Asv.Drones;

public sealed class FtpClientService(IFtpClientEx ftp, ILoggerFactory logFactory)
    : IFtpClientService
{
    private readonly ILogger _log = logFactory.CreateLogger<FtpClientService>();
    private readonly Subject<Unit> _remoteChanged = new();
    private readonly BehaviorSubject<bool> _remoteChanging = new(false);
    private int _busyCounter;

    public Observable<Unit> RemoteChanged => _remoteChanged;
    public Observable<bool> RemoteChanging => _remoteChanging;

    public async Task DownloadFileAsync(
        string remoteFilePath,
        string localDirectory,
        byte partSize = MavlinkFtpHelper.MaxDataSize,
        CancellationToken ct = default,
        IProgress<double>? progress = null
    )
    {
        using var busy = BeginBusyScope();

        await using var stream = new FileStream(
            Path.Combine(localDirectory, GetRemoteFileName(remoteFilePath)),
            FileMode.Create,
            FileAccess.Write,
            FileShare.None
        );

        await ftp.DownloadFile(remoteFilePath, stream, progress, partSize, ct);
        _log.LogInformation("Downloaded {path} -> {dir}", remoteFilePath, localDirectory);
    }

    public async Task BurstDownloadFileAsync(
        string remoteFilePath,
        string localDirectory,
        byte partSize = MavlinkFtpHelper.MaxDataSize,
        CancellationToken ct = default,
        IProgress<double>? progress = null
    )
    {
        using var busy = BeginBusyScope();

        await using var stream = new FileStream(
            Path.Combine(localDirectory, GetRemoteFileName(remoteFilePath)),
            FileMode.Create,
            FileAccess.Write,
            FileShare.None
        );

        await ftp.BurstDownloadFile(remoteFilePath, stream, progress, partSize, ct);
        _log.LogInformation("Burst-Downloaded {path} -> {dir}", remoteFilePath, localDirectory);
    }

    public async Task UploadFileAsync(
        string localFilePath,
        string remoteDirectory,
        CancellationToken ct = default,
        IProgress<double>? progress = null
    )
    {
        using var busy = BeginBusyScope();

        await using var stream = new FileStream(
            localFilePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read
        );

        await ftp.UploadFile(remoteDirectory, stream, progress, ct);
        _log.LogInformation("Uploaded {file} → {target}", localFilePath, remoteDirectory);

        _remoteChanged.OnNext(Unit.Default);
    }

    public async Task RemoveDirectoryAsync(
        string path,
        bool recursive = true,
        CancellationToken ct = default
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
                await ftp.Base.RemoveDirectory(path, ct);
            }

            _log.LogInformation("Directory removed: {path}", path);
        }
        catch (Exception e)
        {
            _log.LogError(e, "Failed to remove directory");
        }
        finally
        {
            _remoteChanged.OnNext(Unit.Default);
        }

        return;

        async Task RemoveDirectoryRecursive(string directoryPath)
        {
            var itemsInDir = ftp.Entries.Where(x => x.Value.ParentPath == directoryPath).ToList();

            foreach (var e in itemsInDir)
            {
                switch (e.Value.Type)
                {
                    case FtpEntryType.Directory:
                        await RemoveDirectoryRecursive(e.Key);
                        break;
                    case FtpEntryType.File:
                        await ftp.Base.RemoveFile(e.Key, ct);
                        break;
                    default:
                        _log.LogError("Unknown FTP entry type: ({type})", e.Value.Type);
                        break;
                }
            }

            await ftp.Base.RemoveDirectory(directoryPath, ct);
        }
    }

    public async Task RemoveFileAsync(string path, CancellationToken ct = default)
    {
        await ftp.Base.RemoveFile(path, ct);
        _log.LogInformation("File removed: {path}", path);
        _remoteChanged.OnNext(Unit.Default);
    }

    public async Task CreateDirectoryAsync(string path, CancellationToken ct = default)
    {
        var folderNumber = 1;
        while (true)
        {
            var name = $"Folder{folderNumber}{MavlinkFtpHelper.DirectorySeparator}";
            ftp.Entries.FirstOrDefault(x => x.Key == $"{path}{name}").Deconstruct(out var k, out _);
            if (!string.IsNullOrEmpty(k))
            {
                folderNumber++;
                continue;
            }

            name =
                path == $"{MavlinkFtpHelper.DirectorySeparator}"
                    ? $"{MavlinkFtpHelper.DirectorySeparator}{name}"
                    : $"{path}{MavlinkFtpHelper.DirectorySeparator}{name}";

            _log.LogInformation("Creating directory '{Path}'", name);
            await ftp.Base.CreateDirectory(name, ct);
            break;
        }
        _remoteChanged.OnNext(Unit.Default);
    }

    public async Task<uint> CalculateCrc32Async(string filePath, CancellationToken ct = default)
    {
        try
        {
            var crc32 = await ftp.Base.CalcFileCrc32(filePath, ct);
            _log.LogInformation("File crc32: {crc32}", crc32);
            return crc32;
        }
        catch (FileNotFoundException e)
        {
            _log.LogError(e, "File '{Path}' not found", filePath);
        }

        return 0U;
    }

    public async Task<string> RenameAsync(
        string oldPath,
        string newName,
        CancellationToken ct = default
    )
    {
        var parentDir = ftp.Entries[oldPath].ParentPath;
        var newPath = MavlinkFtpHelper.Combine(parentDir, newName);

        try
        {
            if (
                ftp.Entries.ContainsKey(newPath)
                || ftp.Entries.ContainsKey(newPath + MavlinkFtpHelper.DirectorySeparator)
            )
            {
                var baseName = Path.GetFileNameWithoutExtension(newName);
                var ext = Path.GetExtension(newName);
                var counter = 1;

                while (
                    ftp.Entries.ContainsKey(newPath)
                    || ftp.Entries.ContainsKey(newPath + MavlinkFtpHelper.DirectorySeparator)
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
            _log.LogError(e, "Failed to rename file. Incorrect path: {Path}", oldPath);
        }

        await ftp.Base.Rename(oldPath, newPath, ct);

        _log.LogInformation("File renamed to '{new}'", newPath);

        _remoteChanged.OnNext(Unit.Default);

        return newPath;
    }

    public async Task<IReadOnlyObservableDictionary<string, IFtpEntry>> Refresh(
        CancellationToken ct = default
    )
    {
        await ftp.Refresh(MavlinkFtpHelper.DirectorySeparator.ToString(), true, ct);
        return ftp.Entries;
    }

    private static string GetRemoteFileName(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return path;
        }

        var slash = path.LastIndexOf(MavlinkFtpHelper.DirectorySeparator);
        return slash >= 0 ? path[(slash + 1)..] : path;
    }

    private IDisposable BeginBusyScope()
    {
        if (Interlocked.Increment(ref _busyCounter) == 1)
        {
            _remoteChanging.OnNext(true);
        }

        return Disposable.Create(() =>
        {
            if (Interlocked.Decrement(ref _busyCounter) == 0)
            {
                _remoteChanging.OnNext(false);
            }
        });
    }

    public void Dispose()
    {
        _remoteChanged.OnCompleted();
        _remoteChanging.OnCompleted();
        ftp.Base.ResetSessions();
    }
}
