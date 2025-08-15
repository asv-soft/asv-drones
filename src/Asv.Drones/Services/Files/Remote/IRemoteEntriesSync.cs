using System;

namespace Asv.Drones;

/// <summary>
/// Synchronizes source dictionary of IFtpEntry into a flat VM list.
/// Applies canonicalization and centralizes add/replace/remove logic.
/// </summary>
public interface IRemoteEntriesSync : IDisposable
{
    /// <summary>Start syncing. Safe to call once; subsequent calls are no-op.</summary>
    void Start();

    /// <summary>Stop syncing and release subscriptions.</summary>
    void Stop();
}
