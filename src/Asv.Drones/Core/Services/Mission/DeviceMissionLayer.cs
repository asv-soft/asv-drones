using Asv.Avalonia;
using Asv.Avalonia.GeoMap;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.Mavlink;
using Asv.Modeling;
using Avalonia.Threading;
using ObservableCollections;
using R3;

namespace Asv.Drones;

public sealed class DeviceMissionLayer : DisposableOnce, IDeviceMissionLayer
{
    private readonly ICollection<IMapAnchor> _mapAnchors;
    private readonly AsvColorKind _iconColor;
    private readonly IExtensionService _ext;
    private readonly MissionPathAnchor _missionPathAnchor;
    private readonly ObservableList<IMissionAnchor> _anchors = [];
    private readonly ReactiveProperty<bool> _isVisible = new(true);
    private readonly ReactiveProperty<bool> _isAnchorsVisible = new(true);
    private readonly ReactiveProperty<bool> _isPathVisible = new(true);
    private readonly CompositeDisposable _disposable = [];

    public DeviceMissionLayer(
        ICollection<IMapAnchor> mapAnchors,
        IClientDevice device,
        AsvColorKind iconColor,
        IExtensionService ext
    )
    {
        _mapAnchors = mapAnchors;
        _iconColor = iconColor;
        _ext = ext;
        DeviceId = device.Id;
        var mission = device.GetRequiredMicroservice<IMissionClientEx>();

        _missionPathAnchor = new MissionPathAnchor(DeviceId, iconColor, _ext, _anchors);
        _mapAnchors.Add(_missionPathAnchor);

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
            .ObserveOnUIThreadDispatcher()
            .Subscribe(_ =>
            {
                ApplyAnchorsVisibility();
                ApplyPathVisibility();
            })
            .DisposeItWith(_disposable);
        _isAnchorsVisible
            .ObserveOnUIThreadDispatcher()
            .Subscribe(_ => ApplyAnchorsVisibility())
            .DisposeItWith(_disposable);
        _isPathVisible
            .ObserveOnUIThreadDispatcher()
            .Subscribe(_ => ApplyPathVisibility())
            .DisposeItWith(_disposable);

        mission
            .MissionItems.PopulateTo(
                _anchors,
                CreateAnchor,
                IsAnchorForMissionItem,
                synchronizationContext: synchronizationContext
            )
            .DisposeItWith(_disposable);
    }

    public DeviceId DeviceId { get; }

    public IReadOnlyObservableList<IMissionAnchor> Anchors => _anchors;

    public ReadOnlyReactiveProperty<bool> IsVisible { get; }

    public ReadOnlyReactiveProperty<bool> IsAnchorsVisible { get; }

    public ReadOnlyReactiveProperty<bool> IsPathVisible { get; }

    public void SwitchAllVisibility()
    {
        var isVisible = !_isVisible.Value;
        _isVisible.Value = isVisible;
        _isAnchorsVisible.Value = isVisible;
        _isPathVisible.Value = isVisible;
    }

    public void SwitchAnchorsVisibility()
    {
        _isAnchorsVisible.Value = !_isAnchorsVisible.Value;
        UpdateAllVisibility();
    }

    public void SwitchPathVisibility()
    {
        _isPathVisible.Value = !_isPathVisible.Value;
        UpdateAllVisibility();
    }

    protected override void InternalDisposeOnce()
    {
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
        var isVisible = IsPathActuallyVisible;
        _missionPathAnchor.IsVisible = isVisible;
    }

    private bool AreAnchorsActuallyVisible => _isVisible.Value && _isAnchorsVisible.Value;

    private bool IsPathActuallyVisible => _isVisible.Value && _isPathVisible.Value;

    private void UpdateAllVisibility()
    {
        _isVisible.Value = _isAnchorsVisible.Value || _isPathVisible.Value;
    }

    private void RemoveAnchorsFromMap()
    {
        _mapAnchors.Remove(_missionPathAnchor);
        _missionPathAnchor.Dispose();

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
