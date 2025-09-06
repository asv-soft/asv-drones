using System;
using System.Linq;
using Asv.Avalonia;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ObservableCollections;
using R3;

namespace Asv.Drones;

/// <summary>
/// Synchronizes source dictionary of IFtpEntry into a flat VM list.
/// Applies canonicalization and centralizes add/replace/remove logic.
/// </summary>
public sealed class RemoteEntriesSync : IDisposable
{
    private readonly ObservableDictionary<string, IFtpEntry> _source;
    private readonly ObservableList<IBrowserItemViewModel> _target;
    private readonly Func<string, IFtpEntry, IBrowserItemViewModel> _factory;
    private readonly ILogger _log;

    private readonly CompositeDisposable _subscriptions = new();
    private readonly char _separator;

    public RemoteEntriesSync(
        ObservableDictionary<string, IFtpEntry> source,
        ObservableList<IBrowserItemViewModel> target,
        Func<string, IFtpEntry, IBrowserItemViewModel> factory,
        ILoggerFactory? loggerFactory = null,
        char separator = MavlinkFtpHelper.DirectorySeparator
    )
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(factory);
        _separator = separator;
        _source = source;
        _target = target;
        _factory = factory;
        _log = (loggerFactory ?? NullLoggerFactory.Instance).CreateLogger<RemoteEntriesSync>();
    }

    /// <summary>Start syncing. Safe to call once; subsequent calls are no-op.</summary>
    public void Start()
    {
        if (_subscriptions.Count > 0)
        {
            return;
        }

        _subscriptions.Add(
            _source
                .ObserveAdd()
                .Subscribe(kv =>
                {
                    var pair = kv.Value;
                    var isDir = pair.Value.Type is FtpEntryType.Directory;
                    var key = FtpBrowserPath.Normalize(pair.Key, isDir, _separator);

                    var existing = _target.FirstOrDefault(i => i.Path == key);
                    if (existing != null)
                    {
                        TrySyncMetadata(existing, pair.Value);
                        return;
                    }

                    _target.Add(_factory(key, pair.Value));
                })
        );

        _subscriptions.Add(
            _source
                .ObserveRemove()
                .Subscribe(kv =>
                {
                    var pair = kv.Value;
                    var isDir = pair.Value.Type is FtpEntryType.Directory;
                    var key = FtpBrowserPath.Normalize(pair.Key, isDir, _separator);

                    var victim = _target.FirstOrDefault(i => i.Path == key);
                    if (victim != null)
                    {
                        _target.Remove(victim);
                    }
                })
        );

        _subscriptions.Add(
            _source
                .ObserveReplace()
                .Subscribe(kv =>
                {
                    var oldPair = kv.OldValue;
                    var newPair = kv.NewValue;

                    var isOldDir = oldPair.Value.Type is FtpEntryType.Directory;
                    var oldKey = FtpBrowserPath.Normalize(oldPair.Key, isOldDir, _separator);

                    var isNewDir = newPair.Value.Type is FtpEntryType.Directory;
                    var newKey = FtpBrowserPath.Normalize(newPair.Key, isNewDir, _separator);

                    var victim =
                        _target.FirstOrDefault(i => i.Path == oldKey)
                        ?? _target.FirstOrDefault(i => i.Path == newKey);

                    if (victim != null)
                    {
                        _target.Remove(victim);
                    }

                    var existing = _target.FirstOrDefault(i => i.Path == newKey);
                    if (existing != null)
                    {
                        TrySyncMetadata(existing, newPair.Value);
                        return;
                    }

                    _target.Add(_factory(newKey, newPair.Value));
                })
        );

        _subscriptions.Add(_source.ObserveReset().Subscribe(_ => _target.RemoveAll()));

        _log.LogDebug("RemoteEntriesSync started");
    }

    /// <summary>Stop syncing and release subscriptions.</summary>
    public void Stop()
    {
        _subscriptions.Dispose();
        _log.LogDebug("RemoteEntriesSync stopped");
    }

    private static void TrySyncMetadata(IBrowserItemViewModel vm, IFtpEntry entry)
    {
        if (vm is BrowserItemViewModel b && entry.Name != b.Name)
        {
            b.Name = entry.Name;
        }
    }

    public void Dispose() => Stop();
}
