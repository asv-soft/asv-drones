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

        position
            .Target.Select(target => target.HasValue)
            .DistinctUntilChanged()
            .ObserveOnUIThreadDispatcher()
            .Subscribe(UpdateVisibility)
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

        UpdateVisibility(position.Target.CurrentValue.HasValue);
    }

    private void UpdateVisibility(bool hasTarget)
    {
        IsVisible = hasTarget;
        if (!hasTarget)
        {
            Location = GeoPoint.NaN;
        }
    }
}
