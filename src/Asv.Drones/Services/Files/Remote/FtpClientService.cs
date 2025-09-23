using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

namespace Asv.Drones;

/// <summary>
/// Unified API for manipulating with files via FTP
/// </summary>
public sealed class FtpClientService(
    IFtpClientEx ftp,
    ILoggerFactory logFactory,
    IFileSystem? fileSystem = null
) : IDisposable
{
    private readonly IFileSystem _fileSystem = fileSystem ?? new FileSystem();
    private readonly ILogger _log = logFactory.CreateLogger<FtpClientService>();
    private readonly Subject<Unit> _remoteChanged = new();
    private readonly BusyFlag _busy = new();

    /// <summary>
    /// Gets <c>true</c> when FTP-operation executes
    /// and <c>false</c> when the client is not busy anymore.
    /// </summary>
    public Observable<bool> RemoteChanging => _busy.IsBusy;

    /// <summary>Gets an observable that emits whenever this service changes the remote file system.</summary>
    public Observable<Unit> RemoteChanged => _remoteChanged;

    public async Task DownloadAsync(
        string from,
        string to,
        FtpEntryType type,
        CancellationToken ct,
        byte partSize = MavlinkFtpHelper.MaxDataSize,
        IProgress<double>? progress = null
    )
    {
        ArgumentException.ThrowIfNullOrEmpty(from);
        ArgumentException.ThrowIfNullOrEmpty(to);

        if (!_fileSystem.Directory.Exists(to))
        {
            _fileSystem.Directory.CreateDirectory(to);
        }

        switch (type)
        {
            case FtpEntryType.File:
                await DownloadFileAsync(from, to, ct, partSize, progress);
                break;
            case FtpEntryType.Directory:
                await DownloadDirectoryAsync(from, to, ct, partSize, progress);
                break;
            default:
                throw new ArgumentOutOfRangeException($"Unsupported FTP entry type: {type}");
        }
    }

    public async Task BurstDownloadAsync(
        string from,
        string to,
        FtpEntryType type,
        CancellationToken ct,
        byte partSize = MavlinkFtpHelper.MaxDataSize,
        IProgress<double>? progress = null
    )
    {
        ArgumentException.ThrowIfNullOrEmpty(from);
        ArgumentException.ThrowIfNullOrEmpty(to);

        if (!_fileSystem.Directory.Exists(to))
        {
            _fileSystem.Directory.CreateDirectory(to);
        }

        switch (type)
        {
            case FtpEntryType.File:
                await BurstDownloadFileAsync(from, to, ct, partSize, progress);
                break;
            case FtpEntryType.Directory:
                await BurstDownloadDirectoryAsync(from, to, ct, partSize, progress);
                break;
            default:
                throw new ArgumentOutOfRangeException($"Unsupported FTP entry type: {type}");
        }
    }

    public async Task UploadAsync(
        string from,
        string to,
        FtpEntryType type,
        CancellationToken ct,
        IProgress<double>? progress = null
    )
    {
        ArgumentException.ThrowIfNullOrEmpty(from);
        ArgumentException.ThrowIfNullOrEmpty(to);

        switch (type)
        {
            case FtpEntryType.File:
                await UploadFileAsync(from, to, ct, progress);
                break;
            case FtpEntryType.Directory:
                await UploadDirectoryAsync(from, to, ct, progress);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    private async Task DownloadFileAsync(
        string remoteFilePath,
        string localDirectory,
        CancellationToken ct,
        byte partSize = MavlinkFtpHelper.MaxDataSize,
        IProgress<double>? progress = null
    )
    {
        using var busyScope = _busy.Enter();
        ct.ThrowIfCancellationRequested();

        await using var stream = new FileStream(
            _fileSystem.Path.Combine(localDirectory, GetRemoteFileName(remoteFilePath)),
            FileMode.Create,
            FileAccess.Write,
            FileShare.None
        );

        await ftp.DownloadFile(remoteFilePath, stream, progress, partSize, ct);
        _log.LogInformation("Downloaded {path} -> {dir}", remoteFilePath, localDirectory);
    }

    private async Task BurstDownloadFileAsync(
        string remoteFilePath,
        string localDirectory,
        CancellationToken ct,
        byte partSize = MavlinkFtpHelper.MaxDataSize,
        IProgress<double>? progress = null
    )
    {
        using var busyScope = _busy.Enter();
        ct.ThrowIfCancellationRequested();

        await using var stream = new FileStream(
            _fileSystem.Path.Combine(localDirectory, GetRemoteFileName(remoteFilePath)),
            FileMode.Create,
            FileAccess.Write,
            FileShare.None
        );

        await ftp.BurstDownloadFile(remoteFilePath, stream, progress, partSize, ct);
        _log.LogInformation("Burst-Downloaded {path} -> {dir}", remoteFilePath, localDirectory);
    }

    private async Task UploadFileAsync(
        string localFilePath,
        string remoteDirectory,
        CancellationToken ct,
        IProgress<double>? progress = null
    )
    {
        using var busyScope = _busy.Enter();
        ct.ThrowIfCancellationRequested();

        await using var stream = new FileStream(
            localFilePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read
        );

        // TODO: sends "(Error to CreateFile: Fail)" after cancellation
        await ftp.UploadFile(remoteDirectory, stream, progress, ct);
        _log.LogInformation("Uploaded {file} -> {target}", localFilePath, remoteDirectory);

        _remoteChanged.OnNext(Unit.Default);
    }

    private async Task DownloadDirectoryAsync(
        string remoteDirectoryPath,
        string localDirectory,
        CancellationToken ct,
        byte partSize = MavlinkFtpHelper.MaxDataSize,
        IProgress<double>? progress = null
    )
    {
        using var busyScope = _busy.Enter();
        ct.ThrowIfCancellationRequested();

        const char dirSep = MavlinkFtpHelper.DirectorySeparator;
        var remoteRoot = remoteDirectoryPath.TrimEnd(dirSep);
        ArgumentException.ThrowIfNullOrEmpty(remoteRoot);

        var remoteRootName = remoteRoot[(remoteRoot.LastIndexOf(dirSep) + 1)..];

        var destRoot = _fileSystem.Path.Combine(localDirectory, remoteRootName);
        _fileSystem.Directory.CreateDirectory(destRoot);

        var remotePrefix = $"{remoteRoot}{dirSep}";

        var dirs = ftp
            .Entries.Where(e =>
                e.Value.Type is FtpEntryType.Directory
                && e.Key.StartsWith(remotePrefix, StringComparison.Ordinal)
            )
            .Select(e => e.Key)
            .Order()
            .ToList();

        var files = ftp
            .Entries.Where(e =>
                e.Value.Type is FtpEntryType.File
                && e.Key.StartsWith(remotePrefix, StringComparison.Ordinal)
            )
            .Select(e => e.Key)
            .Order()
            .ToList();

        foreach (var dir in dirs)
        {
            ct.ThrowIfCancellationRequested();

            var relative = dir[remotePrefix.Length..].TrimEnd(dirSep);
            if (relative.Length == 0)
            {
                continue;
            }

            var localDir = _fileSystem.Path.Combine(
                destRoot,
                relative.Replace(dirSep, _fileSystem.Path.DirectorySeparatorChar)
            );

            _fileSystem.Directory.CreateDirectory(localDir);
        }

        var total = files.Count;
        if (total == 0)
        {
            progress?.Report(1.0);
            _log.LogInformation(
                "Remote directory '{remote}' contains no files",
                remoteDirectoryPath
            );
            return;
        }

        var completed = 0;
        foreach (var file in files)
        {
            ct.ThrowIfCancellationRequested();

            var relative = file[remotePrefix.Length..];
            var localFileDir = _fileSystem.Path.Combine(
                destRoot,
                _fileSystem
                    .Path.GetDirectoryName(relative)!
                    .Replace(dirSep, _fileSystem.Path.DirectorySeparatorChar)
            );

            _fileSystem.Directory.CreateDirectory(localFileDir);

            var completedSafe = completed;
            var nested = progress is null
                ? null
                : new Progress<double>(p => progress.Report((completedSafe + p) / total));

            await DownloadFileAsync(file, localFileDir, ct, partSize, nested);

            completed++;
            progress?.Report((double)completed / total);
        }

        _log.LogInformation(
            "Downloaded directory '{remote}' -> '{dest}' (files: {count})",
            remoteDirectoryPath,
            destRoot,
            total
        );
    }

    private async Task BurstDownloadDirectoryAsync(
        string remoteDirectoryPath,
        string localDirectory,
        CancellationToken ct,
        byte partSize = MavlinkFtpHelper.MaxDataSize,
        IProgress<double>? progress = null
    )
    {
        using var busyScope = _busy.Enter();
        ct.ThrowIfCancellationRequested();

        const char dirSep = MavlinkFtpHelper.DirectorySeparator;
        var remoteRoot = remoteDirectoryPath.TrimEnd(dirSep);
        ArgumentException.ThrowIfNullOrEmpty(remoteRoot);

        var remoteRootName = remoteRoot[(remoteRoot.LastIndexOf(dirSep) + 1)..];

        var destRoot = _fileSystem.Path.Combine(localDirectory, remoteRootName);
        _fileSystem.Directory.CreateDirectory(destRoot);

        var remotePrefix = $"{remoteRoot}{dirSep}";

        var dirs = ftp
            .Entries.Where(e =>
                e.Value.Type == FtpEntryType.Directory
                && e.Key.StartsWith(remotePrefix, StringComparison.Ordinal)
            )
            .Select(e => e.Key)
            .Order()
            .ToList();

        var files = ftp
            .Entries.Where(e =>
                e.Value.Type == FtpEntryType.File
                && e.Key.StartsWith(remotePrefix, StringComparison.Ordinal)
            )
            .Select(e => e.Key)
            .Order()
            .ToList();

        foreach (var dir in dirs)
        {
            ct.ThrowIfCancellationRequested();

            var relative = dir[remotePrefix.Length..].TrimEnd(dirSep);
            if (relative.Length == 0)
            {
                continue;
            }

            var localDir = _fileSystem.Path.Combine(
                destRoot,
                relative.Replace(dirSep, _fileSystem.Path.DirectorySeparatorChar)
            );

            _fileSystem.Directory.CreateDirectory(localDir);
        }

        var total = files.Count;
        if (total == 0)
        {
            progress?.Report(1.0);
            _log.LogInformation(
                "Remote directory '{remote}' contains no files",
                remoteDirectoryPath
            );
            return;
        }

        var completed = 0;
        foreach (var file in files)
        {
            ct.ThrowIfCancellationRequested();

            var relative = file[remotePrefix.Length..];
            var localFileDir = _fileSystem.Path.Combine(
                destRoot,
                _fileSystem
                    .Path.GetDirectoryName(relative)!
                    .Replace(dirSep, _fileSystem.Path.DirectorySeparatorChar)
            );

            _fileSystem.Directory.CreateDirectory(localFileDir);

            var completedSafe = completed;
            var nested = progress is null
                ? null
                : new Progress<double>(p => progress.Report((completedSafe + p) / total));

            // TODO: sends "(Timeout to execute 'FILE_TRANSFER_PROTOCOL')" when tries to TerminateSession(0)
            await BurstDownloadFileAsync(file, localFileDir, ct, partSize, nested);

            completed++;
            progress?.Report((double)completed / total);
        }

        _log.LogInformation(
            "Burst-Downloaded directory '{remote}' -> '{dest}' (files: {count})",
            remoteDirectoryPath,
            destRoot,
            total
        );
    }

    private async Task UploadDirectoryAsync(
        string localDirectoryPath,
        string remoteDirectory,
        CancellationToken ct,
        IProgress<double>? progress = null
    )
    {
        using var busyScope = _busy.Enter();
        ct.ThrowIfCancellationRequested();

        var localRoot = new DirectoryInfo(localDirectoryPath);
        if (!localRoot.Exists)
        {
            throw new DirectoryNotFoundException(localDirectoryPath);
        }

        const char rSep = MavlinkFtpHelper.DirectorySeparator;
        var rootName = localRoot.Name;

        var remoteDirNorm = remoteDirectory;
        if (!remoteDirNorm.EndsWith(rSep))
        {
            remoteDirNorm += rSep;
        }

        var lastSegment = remoteDirNorm
            .TrimEnd(rSep)
            .Split(rSep, StringSplitOptions.RemoveEmptyEntries)
            .LastOrDefault();

        var remoteRoot = string.Equals(lastSegment, rootName, StringComparison.Ordinal)
            ? remoteDirNorm
            : $"{remoteDirNorm}{rootName}{rSep}";

        await ftp.Base.CreateDirectory(remoteRoot, ct);

        var localDirs = _fileSystem.Directory.GetDirectories(
            localDirectoryPath,
            "*",
            SearchOption.AllDirectories
        );

        foreach (var dir in localDirs)
        {
            var rel = _fileSystem
                .Path.GetRelativePath(localDirectoryPath, dir)
                .Replace(_fileSystem.Path.DirectorySeparatorChar, rSep);

            var remoteDir = MavlinkFtpHelper.Combine(remoteRoot, $"{rel}{rSep}");

            await ftp.Base.CreateDirectory(remoteDir, ct);
        }

        var localFiles = _fileSystem.Directory.GetFiles(
            localDirectoryPath,
            "*",
            SearchOption.AllDirectories
        );
        var totalBytes = localFiles.Sum(f => new FileInfo(f).Length);
        long uploaded = 0;

        foreach (var file in localFiles)
        {
            ct.ThrowIfCancellationRequested();

            var rel = _fileSystem
                .Path.GetRelativePath(localDirectoryPath, file)
                .Replace(_fileSystem.Path.DirectorySeparatorChar, rSep);

            var remoteFilePath = MavlinkFtpHelper.Combine(remoteRoot, rel);

            await using var stream = new FileStream(
                file,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read
            );

            var fileLength = stream.Length;
            var uploadedSafe = uploaded;
            var fileProgress =
                progress == null
                    ? null
                    : new Progress<double>(p =>
                    {
                        var current = uploadedSafe + (long)(fileLength * p);
                        progress.Report(current / (double)totalBytes);
                    });

            await ftp.UploadFile(remoteFilePath, stream, fileProgress, ct);

            uploaded += fileLength;
            progress?.Report(uploaded / (double)totalBytes);
            _log.LogInformation("Uploaded {File} -> {Target}", file, remoteFilePath);
        }

        _log.LogInformation(
            "Uploaded folder {local} -> {remote} (files: {count})",
            localDirectoryPath,
            remoteDirectory,
            localFiles.Length
        );

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
        string newPath,
        CancellationToken ct = default
    )
    {
        var parentDir = ftp.Entries[oldPath].ParentPath;

        try
        {
            if (ftp.Entries.ContainsKey(newPath))
            {
                var fullName = MavlinkFtpHelper.GetFileName(newPath);
                var baseName = _fileSystem.Path.GetFileNameWithoutExtension(fullName);
                var ext = _fileSystem.Path.GetExtension(fullName);
                var counter = 1;

                while (ftp.Entries.ContainsKey(newPath))
                {
                    newPath = MavlinkFtpHelper.Combine(parentDir, $"{baseName} ({counter++}){ext}");
                }
            }
            else
            {
                await ftp.Base.Rename(oldPath, newPath, ct);
                _log.LogInformation("File renamed to '{new}'", newPath);
            }
        }
        catch (FileNotFoundException e)
        {
            _log.LogError(e, "Failed to rename file. Incorrect path: {Path}", oldPath);
        }

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

    public void Dispose()
    {
        _busy.Dispose();
        _remoteChanged.OnCompleted();
        ftp.Base.ResetSessions();
    }
}
