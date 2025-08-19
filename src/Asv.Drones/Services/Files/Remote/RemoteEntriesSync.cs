using System;
using System.Linq;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ObservableCollections;
using R3;

namespace Asv.Drones;

/// <inheritdoc />
public sealed class RemoteEntriesSync(
    ObservableDictionary<string, IFtpEntry> source,
    ObservableList<IBrowserItemViewModel> target,
    Func<string, IFtpEntry, IBrowserItemViewModel> factory,
    ILoggerFactory? loggerFactory = null,
    char separator = MavlinkFtpHelper.DirectorySeparator
) : IRemoteEntriesSync
{
    private readonly ObservableDictionary<string, IFtpEntry> _source =
        source ?? throw new ArgumentNullException(nameof(source));
    private readonly ObservableList<IBrowserItemViewModel> _target =
        target ?? throw new ArgumentNullException(nameof(target));
    private readonly Func<string, IFtpEntry, IBrowserItemViewModel> _factory =
        factory ?? throw new ArgumentNullException(nameof(factory));
    private readonly ILogger _log = (
        loggerFactory ?? NullLoggerFactory.Instance
    ).CreateLogger<RemoteEntriesSync>();

    private readonly CompositeDisposable _subscriptions = new();

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
                    var key = BrowserPathRules.Normalize(pair.Key, isDir, separator);

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
                    var key = BrowserPathRules.Normalize(pair.Key, isDir, separator);

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
                    var oldKey = BrowserPathRules.Normalize(oldPair.Key, isOldDir, separator);

                    var isNewDir = newPair.Value.Type is FtpEntryType.Directory;
                    var newKey = BrowserPathRules.Normalize(newPair.Key, isNewDir, separator);

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

        _subscriptions.Add(_source.ObserveReset().Subscribe(_ => _target.Clear()));

        _log.LogDebug("RemoteEntriesSync started");
    }

    public void Stop()
    {
        _subscriptions.Dispose();
        _log.LogDebug("RemoteEntriesSync stopped");
    }

    public void Dispose() => Stop();

    private static void TrySyncMetadata(IBrowserItemViewModel vm, IFtpEntry entry)
    {
        if (vm is BrowserItemViewModel b && entry.Name != b.Name)
        {
            b.Name = entry.Name;
        }
    }
}
