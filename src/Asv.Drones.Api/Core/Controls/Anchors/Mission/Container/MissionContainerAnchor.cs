using Asv.Avalonia;
using Asv.Avalonia.GeoMap;
using Asv.Avalonia.IO;
using Asv.Common;
using Asv.Drones.Api;
using Asv.IO;
using Asv.IO.Device;
using Asv.Mavlink;
using Asv.Modeling;
using Avalonia.Threading;
using ObservableCollections;
using R3;

namespace Asv.Drones;

public sealed class MissionContainerAnchor
    : MapAnchor<MissionContainerAnchor>,
        IMissionContainerAnchor
{
    public const string MissionRootAnchorIdBase = "mission-container";

    private readonly ICollection<IMapAnchor> _mapAnchors;
    private readonly AsvColorKind _iconColor;
    private readonly IExtensionService _ext;
    private readonly MissionPathAnchor _missionPathAnchor;
    private readonly ObservableList<IMissionAnchor> _anchors = [];
    private readonly ReactiveProperty<bool> _isMissionVisible = new(true);
    private readonly ReactiveProperty<bool> _isAnchorsVisible = new(true);
    private readonly ReactiveProperty<bool> _isPathVisible = new(true);

    public MissionContainerAnchor()
        : base(DesignTime.Id.TypeId, DesignTime.ExtensionService)
    {
        DesignTime.ThrowIfNotDesignMode();

        _mapAnchors = [];
        _iconColor = AsvColorKind.Info5;
        _ext = DesignTime.ExtensionService;
        DeviceId = new ExampleDeviceId("design", 0);
        _missionPathAnchor = new MissionPathAnchor().DisposeItWith(Disposable);

        IsMissionVisible = _isMissionVisible.ToReadOnlyReactiveProperty().DisposeItWith(Disposable);
        IsAnchorsVisible = _isAnchorsVisible.ToReadOnlyReactiveProperty().DisposeItWith(Disposable);
        IsPathVisible = _isPathVisible.ToReadOnlyReactiveProperty().DisposeItWith(Disposable);
    }

    public MissionContainerAnchor(
        ICollection<IMapAnchor> mapAnchors,
        IClientDevice device,
        AsvColorKind iconColor,
        IExtensionService ext
    )
        : base(MissionRootAnchorIdBase, BuildArgs(device.Id), ext)
    {
        _mapAnchors = mapAnchors;
        _iconColor = iconColor;
        _ext = ext;
        DeviceId = device.Id;

        IsVisible = false;
        IsAnnotationVisible = false;
        IsReadOnly = true;
        IconSize = 0;
        Location = GeoPoint.NaN;

        var mission = device.GetRequiredMicroservice<IMissionClientEx>();

        _missionPathAnchor = new MissionPathAnchor(DeviceId, iconColor, _ext, _anchors);
        _mapAnchors.Add(_missionPathAnchor);

        IsMissionVisible = _isMissionVisible.ToReadOnlyReactiveProperty().DisposeItWith(Disposable);
        IsAnchorsVisible = _isAnchorsVisible.ToReadOnlyReactiveProperty().DisposeItWith(Disposable);
        IsPathVisible = _isPathVisible.ToReadOnlyReactiveProperty().DisposeItWith(Disposable);
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
            .DisposeItWith(Disposable);

        _anchors.DisposeRemovedItems().DisposeItWith(Disposable);
        _isMissionVisible
            .ObserveOnUIThreadDispatcher()
            .Subscribe(_ =>
            {
                ApplyAnchorsVisibility();
                ApplyPathVisibility();
            })
            .DisposeItWith(Disposable);
        _isAnchorsVisible
            .ObserveOnUIThreadDispatcher()
            .Subscribe(_ => ApplyAnchorsVisibility())
            .DisposeItWith(Disposable);
        _isPathVisible
            .ObserveOnUIThreadDispatcher()
            .Subscribe(_ => ApplyPathVisibility())
            .DisposeItWith(Disposable);

        mission
            .MissionItems.PopulateTo(
                _anchors,
                CreateAnchor,
                IsAnchorForMissionItem,
                synchronizationContext: synchronizationContext
            )
            .DisposeItWith(Disposable);

        Disposable.AddAction(DisposeMissionAnchors);
    }

    public DeviceId DeviceId { get; }

    public IReadOnlyObservableList<IMissionAnchor> Anchors => _anchors;

    public ReadOnlyReactiveProperty<bool> IsMissionVisible { get; }

    public ReadOnlyReactiveProperty<bool> IsAnchorsVisible { get; }

    public ReadOnlyReactiveProperty<bool> IsPathVisible { get; }

    public void SwitchAllVisibility()
    {
        var isVisible = !_isMissionVisible.Value;
        _isMissionVisible.Value = isVisible;
        _isAnchorsVisible.Value = isVisible;
        _isPathVisible.Value = isVisible;
    }

    public void SwitchAnchorsVisibility()
    {
        SetAnchorsVisibility(!_isAnchorsVisible.Value);
    }

    public void SwitchPathVisibility()
    {
        SetPathVisibility(!_isPathVisible.Value);
    }

    protected override void AfterLoadExtensions()
    {
        RegisterLayout();
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

    private bool AreAnchorsActuallyVisible => _isMissionVisible.Value && _isAnchorsVisible.Value;

    private bool IsPathActuallyVisible => _isMissionVisible.Value && _isPathVisible.Value;

    private void UpdateAllVisibility()
    {
        _isMissionVisible.Value = _isAnchorsVisible.Value || _isPathVisible.Value;
    }

    private void DisposeMissionAnchors()
    {
        _mapAnchors.Remove(this);
        _mapAnchors.Remove(_missionPathAnchor);
        _missionPathAnchor.Dispose();

        foreach (var anchor in _anchors.ToArray())
        {
            _mapAnchors.Remove(anchor);
        }

        _anchors.ClearWithItemsDispose();
    }

    private void SetAnchorsVisibility(bool isVisible)
    {
        _isAnchorsVisible.Value = isVisible;
        UpdateAllVisibility();
    }

    private void SetPathVisibility(bool isVisible)
    {
        _isPathVisible.Value = isVisible;
        UpdateAllVisibility();
    }

    private void RegisterLayout()
    {
        Layout
            .Register(
                nameof(IsAnchorsVisible),
                SetAnchorsVisibility,
                () => _isAnchorsVisible.Value,
                _isAnchorsVisible.Skip(1)
            )
            .DisposeItWith(Disposable);

        Layout
            .Register(
                nameof(IsPathVisible),
                SetPathVisibility,
                () => _isPathVisible.Value,
                _isPathVisible.Skip(1)
            )
            .DisposeItWith(Disposable);

        Layout.LoadWhenRootAttached(RootTracking).AddTo(ref DisposableBag);
    }

    private static bool IsMapAnchor(IMissionAnchor source, IMissionAnchor target)
    {
        return target.DeviceId == source.DeviceId && target.MissionIndex == source.MissionIndex;
    }

    private static NavArgs BuildArgs(DeviceId deviceId)
    {
        return new NavArgs([
            new KeyValuePair<string, string?>(
                DevicePageViewModelMixin.ArgsDeviceIdKey,
                deviceId.AsString()
            ),
        ]);
    }
}
