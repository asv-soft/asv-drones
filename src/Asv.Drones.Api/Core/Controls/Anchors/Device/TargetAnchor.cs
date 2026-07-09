using Asv.Avalonia;
using Asv.Avalonia.IO;
using Asv.Common;
using Asv.IO;
using Asv.Mavlink;
using Avalonia.Media;
using Material.Icons;
using R3;

namespace Asv.Drones.Api;

public sealed class TargetAnchor : DeviceAnchor<TargetAnchor>
{
    public const string AnchorIdBase = "target-path";
    private const double TargetReachedDistanceMeters = 1.0;

    private bool _isTargetReached;
    private GeoPoint? _lastTarget;

    public TargetAnchor()
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public TargetAnchor(IClientDevice device, IDeviceManager manager, IExtensionService ext)
        : base(AnchorIdBase, [], manager, device, ext)
    {
        Icon = MaterialIconKind.Target;
        IsAnnotationVisible = false;

        PolygonPen = new Pen(
            manager.GetDeviceColor(device.Id).ToBrush(),
            thickness: 3,
            dashStyle: DashStyle.Dash
        );

        IsReadOnly = true;
        IsVisible = false;
        Location = GeoPoint.NaN;

        var position = device.GetRequiredMicroservice<IPositionClientEx>();
        _lastTarget = position.Target.CurrentValue;

        position
            .Target.CombineLatest(position.TargetDistance, (_, _) => Unit.Default)
            .ThrottleLast(TimeSpan.FromMilliseconds(200))
            .ObserveOnUIThreadDispatcher()
            .Subscribe(_ => UpdateVisibility(position))
            .DisposeItWith(Disposable);

        position
            .Target.Where(target => target.HasValue)
            .Cast<GeoPoint?, GeoPoint>()
            .DistinctUntilChanged()
            .ObserveOnUIThreadDispatcher()
            .ThrottleLast(TimeSpan.FromMilliseconds(200))
            .Subscribe(target => Location = target)
            .DisposeItWith(Disposable);

        this.DrawLine(
                position
                    .Current.DistinctUntilChanged()
                    .ThrottleLast(TimeSpan.FromMilliseconds(200))
                    .ObserveOnUIThreadDispatcher(),
                position
                    .Target.Where(target => target.HasValue)
                    .Cast<GeoPoint?, GeoPoint>()
                    .DistinctUntilChanged()
                    .ThrottleLast(TimeSpan.FromMilliseconds(200))
                    .ObserveOnUIThreadDispatcher()
            )
            .DisposeItWith(Disposable);

        UpdateVisibility(position);
    }

    private void UpdateVisibility(IPositionClientEx position)
    {
        var target = position.Target.CurrentValue;
        var targetChanged = target != _lastTarget;
        if (targetChanged)
        {
            _lastTarget = target;
            _isTargetReached = false;
        }

        var distance = position.TargetDistance.CurrentValue;
        if (
            target.HasValue
            && !targetChanged
            && !_isTargetReached
            && double.IsFinite(distance)
            && distance <= TargetReachedDistanceMeters
        )
        {
            _isTargetReached = true;
        }

        IsVisible = target.HasValue && !_isTargetReached;
        if (!IsVisible)
        {
            Location = GeoPoint.NaN;
        }
    }
}
