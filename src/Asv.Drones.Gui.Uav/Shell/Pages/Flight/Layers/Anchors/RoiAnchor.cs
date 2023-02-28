using System.Reactive.Disposables;
using System.Reactive.Linq;
using Asv.Avalonia.Map;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Avalonia.Media;
using Material.Icons;
using ReactiveUI;

namespace Asv.Drones.Gui.Uav
{
    public class RoiAnchor : MapAnchorBase
    {
        public RoiAnchor(IVehicle vehicle) : base(new(UavWellKnownUri.UavAnchorsBaseUri + $"/{vehicle.FullId}/roi"))
        {
            Size = 32;
            OffsetX = OffsetXEnum.Center;
            OffsetY = OffsetYEnum.Center;
            Icon = MaterialIconKind.ImageFilterCenterFocus;
            IconBrush = Brushes.LightSeaGreen;
            IsVisible = false;

            vehicle.Roi.Select(_ => _.HasValue).DistinctUntilChanged().Subscribe(_ => IsVisible = _).DisposeWith(Disposable);
            vehicle.Roi.Where(_ => _.HasValue).Subscribe(_ => Location = _.Value).DisposeWith(Disposable);

            vehicle.Name.Subscribe(_ => Title = $"ROI of {_}").DisposeWith(Disposable); // TODO: Localize

            vehicle.Yaw.Sample(TimeSpan.FromSeconds(1), RxApp.MainThreadScheduler).Subscribe(_ => Description = $"Yaw angle {_:F0} deg"); // TODO: Localize
        }

    }
}