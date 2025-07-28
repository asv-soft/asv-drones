using System;
using System.Threading;
using System.Threading.Tasks;
using Asv.Mavlink;
using ObservableCollections;
using R3;

namespace Asv.Drones;

/// <summary>
/// Unified API for manipulating with files via FTP
/// </summary>
public interface IFtpClientService : IDisposable
{
    /// <summary>Gets is remote side changed by this service.</summary>
    Observable<Unit> RemoteChanged { get; }

    /// <summary>
    /// Gets <c>true</c> when FTP-operation executes
    /// and <c>false</c> when client is not busy anymore.
    /// </summary>
    Observable<bool> RemoteChanging { get; }

    Task DownloadFileAsync(
        string remoteFilePath,
        string localDirectory,
        byte partSize = MavlinkFtpHelper.MaxDataSize,
        CancellationToken ct = default,
        IProgress<double>? progress = null
    );

    Task BurstDownloadFileAsync(
        string remoteFilePath,
        string localDirectory,
        byte partSize = MavlinkFtpHelper.MaxDataSize,
        CancellationToken ct = default,
        IProgress<double>? progress = null
    );

    Task UploadFileAsync(
        string localFilePath,
        string remoteDirectory,
        CancellationToken ct = default,
        IProgress<double>? progress = null
    );

    Task DownloadDirectoryAsync(
        string remoteDirectoryPath,
        string localDirectory,
        byte partSize = MavlinkFtpHelper.MaxDataSize,
        CancellationToken ct = default,
        IProgress<double>? progress = null
    );

    Task BurstDownloadDirectoryAsync(
        string remoteDirectoryPath,
        string localDirectory,
        byte partSize = MavlinkFtpHelper.MaxDataSize,
        CancellationToken ct = default,
        IProgress<double>? progress = null
    );

    Task UploadDirectoryAsync(
        string localDirectoryPath,
        string remoteDirectory,
        CancellationToken ct = default,
        IProgress<double>? progress = null
    );

    Task RemoveDirectoryAsync(string path, bool recursive = true, CancellationToken ct = default);

    Task RemoveFileAsync(string path, CancellationToken ct = default);

    Task<uint> CalculateCrc32Async(string filePath, CancellationToken ct = default);

    Task<string> RenameAsync(string oldPath, string newName, CancellationToken ct = default);

    Task<IReadOnlyObservableDictionary<string, IFtpEntry>> Refresh(CancellationToken ct = default);

    Task CreateDirectoryAsync(string path, CancellationToken ct = default);
}
