using Asv.Avalonia;
using Asv.Avalonia.GeoMap;
using Asv.Avalonia.IO;
using Asv.Common;
using Asv.IO;
using Asv.IO.Device;
using Asv.Modeling;
using Avalonia.Media;
using ObservableCollections;
using R3;

namespace Asv.Drones.Api;

public sealed class MissionPathAnchor : MapAnchor<MissionPathAnchor>
{
    public const string MissionPathAnchorIdBase = "mission-path";
    private const int MinValidPoints = 2;
    private readonly IReadOnlyObservableList<IMissionAnchor> _anchors;
    private readonly SerialDisposable _pathPointSubscriptions = new();

    public MissionPathAnchor()
        : base(DesignTime.Id.TypeId, DesignTime.ExtensionService)
    {
        DesignTime.ThrowIfNotDesignMode();
        DeviceId = new ExampleDeviceId("design", 0);

        _anchors = new ObservableList<IMissionAnchor>();
        _pathPointSubscriptions.DisposeItWith(Disposable);
    }

    public MissionPathAnchor(
        DeviceId deviceId,
        AsvColorKind pathColor,
        IExtensionService ext,
        IReadOnlyObservableList<IMissionAnchor> anchors
    )
        : base(MissionPathAnchorIdBase, BuildArgs(deviceId), ext)
    {
        DeviceId = deviceId;
        _anchors = anchors;
        IsAnnotationVisible = false;
        IsReadOnly = true;
        IconSize = 0;
        Location = GeoPoint.NaN;
        PolygonPen = new Pen(pathColor.ToBrush());

        _pathPointSubscriptions.DisposeItWith(Disposable);

        this.ObservePropertyChanged(anchor => anchor.IsVisible)
            .Skip(1)
            .ObserveOnUIThreadDispatcher()
            .Subscribe(_ => TryUpdatePath())
            .DisposeItWith(Disposable);

        anchors
            .ObserveChanged()
            .ObserveOnUIThreadDispatcher()
            .ThrottleLast(TimeSpan.FromMilliseconds(100))
            .Subscribe(_ => Refresh())
            .DisposeItWith(Disposable);

        Refresh();
    }

    public DeviceId DeviceId { get; }

    private void Refresh()
    {
        _pathPointSubscriptions.Disposable = null;
        if (!TryUpdatePath())
        {
            return;
        }

        DisposableBag subscriptions = default;
        foreach (var anchor in _anchors)
        {
            anchor
                .MissionItem.Location.Skip(1)
                .DistinctUntilChanged()
                .ObserveOnUIThreadDispatcher()
                .ThrottleLast(TimeSpan.FromMilliseconds(200))
                .Subscribe(_ => TryUpdatePath())
                .AddTo(ref subscriptions);
        }

        _pathPointSubscriptions.Disposable = subscriptions;
    }

    private bool TryUpdatePath()
    {
        Polygon.Clear();

        if (!IsVisible || _anchors.Count < MinValidPoints)
        {
            return false;
        }

        var orderedAnchors = _anchors
            .OrderBy(anchor => anchor.MissionIndex)
            .Select(a => a.MissionItem.Location.CurrentValue)
            .ToArray();

        Polygon.AddRange(orderedAnchors);

        return true;
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
