using Asv.Avalonia;
using Asv.Avalonia.GeoMap;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Asv.Modeling;
using Avalonia.Threading;
using Material.Icons;
using ObservableCollections;
using R3;

namespace Asv.Drones;

public sealed class DeviceMissionLayer : DisposableOnce, IDeviceMissionLayer
{
    private readonly ICollection<IMapAnchor> _mapAnchors;
    private readonly AsvColorKind _iconColor;
    private readonly IExtensionService _ext;
    private readonly IMissionClientEx _mission;
    private readonly ObservableList<IMissionAnchor> _anchors = [];
    private readonly ReactiveProperty<bool> _isVisible = new(true);
    private readonly ReactiveProperty<bool> _isAnchorsVisible = new(true);
    private readonly ReactiveProperty<bool> _isPathVisible = new(true);
    private readonly CompositeDisposable _disposable = [];
    private readonly SerialDisposable _missionPathSubscriptions = new();
    private IMissionAnchor? _highlightedAnchor;

    public DeviceMissionLayer(
        ICollection<IMapAnchor> mapAnchors,
        IClientDevice device,
        IMissionClientEx mission,
        AsvColorKind iconColor,
        IExtensionService ext
    )
    {
        _mapAnchors = mapAnchors;
        _iconColor = iconColor;
        _ext = ext;
        _mission = mission;
        DeviceId = device.Id;
        IsVisible = _isVisible.ToReadOnlyReactiveProperty().DisposeItWith(_disposable);
        IsAnchorsVisible = _isAnchorsVisible
            .ToReadOnlyReactiveProperty()
            .DisposeItWith(_disposable);
        IsPathVisible = _isPathVisible.ToReadOnlyReactiveProperty().DisposeItWith(_disposable);
        var synchronizationContext = new AvaloniaSynchronizationContext(
            Dispatcher.UIThread,
            DispatcherPriority.Default
        );

        _anchors
            .PopulateTo<IMissionAnchor, IMapAnchor, IMissionAnchor>(
                _mapAnchors,
                anchor => anchor,
                IsMapAnchor,
                synchronizationContext: synchronizationContext
            )
            .DisposeItWith(_disposable);

        _anchors.DisposeRemovedItems().DisposeItWith(_disposable);
        _isVisible
            .Subscribe(_ =>
            {
                ApplyAnchorsVisibility();
                ApplyPathVisibility();
            })
            .DisposeItWith(_disposable);
        _isAnchorsVisible.Subscribe(_ => ApplyAnchorsVisibility()).DisposeItWith(_disposable);
        _isPathVisible.Subscribe(_ => ApplyPathVisibility()).DisposeItWith(_disposable);
        _missionPathSubscriptions.DisposeItWith(_disposable);
        Observable
            .Merge(
                _anchors.ObserveAdd().Select(_ => Unit.Default),
                _anchors.ObserveRemove().Select(_ => Unit.Default),
                _mission.Current.Select(_ => Unit.Default),
                _mission.MissionItems.ObserveChanged().Select(_ => Unit.Default)
            )
            .ObserveOnUIThreadDispatcher()
            .ThrottleLast(TimeSpan.FromMilliseconds(16))
            .Subscribe(_ =>
            {
                if (IsPathActuallyVisible)
                {
                    RebuildMissionPath();
                }

                UpdateHighlightedAnchor();
            })
            .DisposeItWith(_disposable);

        _mission
            .MissionItems.PopulateTo(
                _anchors,
                CreateAnchor,
                IsAnchorForMissionItem,
                synchronizationContext: synchronizationContext
            )
            .DisposeItWith(_disposable);

        UpdateHighlightedAnchor();
    }

    public DeviceId DeviceId { get; }

    public IReadOnlyObservableList<IMissionAnchor> Anchors => _anchors;

    public ReadOnlyReactiveProperty<bool> IsVisible { get; }

    public ReadOnlyReactiveProperty<bool> IsAnchorsVisible { get; }

    public ReadOnlyReactiveProperty<bool> IsPathVisible { get; }

    public void SwitchAllVisibility()
    {
        _isVisible.Value = !_isVisible.Value;
    }

    public void SwitchAnchorsVisibility()
    {
        _isAnchorsVisible.Value = !_isAnchorsVisible.Value;
    }

    public void SwitchPathVisibility()
    {
        _isPathVisible.Value = !_isPathVisible.Value;
    }

    protected override void InternalDisposeOnce()
    {
        _missionPathSubscriptions.Disposable = null;
        ResetHighlightedAnchor();
        RemoveAnchorsFromMap();
        _anchors.ClearWithItemsDispose();
        _disposable.Dispose();
    }

    private MissionAnchor CreateAnchor(MissionItem missionItem)
    {
        var anchor = new MissionAnchor(DeviceId, missionItem, _iconColor, _ext)
        {
            IsVisible = AreAnchorsActuallyVisible,
            IsAnnotationVisible = AreAnchorsActuallyVisible,
        };

        return anchor;
    }

    private static bool IsAnchorForMissionItem(MissionItem missionItem, IMissionAnchor anchor)
    {
        return anchor.MissionIndex == missionItem.Index;
    }

    private void RebuildMissionPath()
    {
        _missionPathSubscriptions.Disposable = null;
        ClearMissionPath();

        var anchors = _anchors.OrderBy(anchor => anchor.MissionIndex).ToArray();
        DisposableBag subscriptions = default;

        for (var i = 0; i < anchors.Length - 1; i++)
        {
            anchors[i]
                .DrawLine(anchors[i].MissionItem.Location, anchors[i + 1].MissionItem.Location)
                .AddTo(ref subscriptions);
        }

        _missionPathSubscriptions.Disposable = subscriptions;
    }

    private void ClearMissionPath()
    {
        foreach (var anchor in _anchors)
        {
            anchor.Polygon.Clear();
        }
    }

    private void ApplyAnchorsVisibility()
    {
        var isVisible = AreAnchorsActuallyVisible;
        foreach (var anchor in _anchors)
        {
            anchor.IsVisible = isVisible;
            anchor.IsAnnotationVisible = isVisible;
        }
    }

    private void ApplyPathVisibility()
    {
        if (!IsPathActuallyVisible)
        {
            _missionPathSubscriptions.Disposable = null;
            ClearMissionPath();
            return;
        }

        RebuildMissionPath();
    }

    private bool AreAnchorsActuallyVisible => _isVisible.Value && _isAnchorsVisible.Value;

    private bool IsPathActuallyVisible => _isVisible.Value && _isPathVisible.Value;

    private void UpdateHighlightedAnchor()
    {
        var anchor = _anchors.FirstOrDefault(anchor =>
            anchor.MissionIndex == _mission.Current.CurrentValue
        );
        if (ReferenceEquals(_highlightedAnchor, anchor))
        {
            return;
        }

        ResetHighlightedAnchor();
        _highlightedAnchor = anchor;
        ApplyHighlightedAnchor();
    }

    private void ApplyHighlightedAnchor()
    {
        if (_highlightedAnchor is null)
        {
            return;
        }

        _highlightedAnchor.Icon = MaterialIconKind.Target;
        _highlightedAnchor.IconColor = AsvColorKind.Info5;
    }

    private void ResetHighlightedAnchor()
    {
        var anchor = _highlightedAnchor;
        _highlightedAnchor = null;
        if (anchor is null || _anchors.Contains(anchor) == false)
        {
            return;
        }

        anchor.Icon = MaterialIconKind.StarFourPointsSmall;
        anchor.IconColor = _iconColor;
    }

    private void RemoveAnchorsFromMap()
    {
        foreach (var anchor in _anchors.ToArray())
        {
            _mapAnchors.Remove(anchor);
        }
    }

    private static bool IsMapAnchor(IMissionAnchor source, IMissionAnchor target)
    {
        return target.DeviceId == source.DeviceId && target.MissionIndex == source.MissionIndex;
    }
}
