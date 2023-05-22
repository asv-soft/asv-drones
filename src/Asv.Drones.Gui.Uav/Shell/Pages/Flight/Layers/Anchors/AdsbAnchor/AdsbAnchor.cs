using System.Reactive.Linq;
using Asv.Avalonia.Map;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Asv.Mavlink.Vehicle;
using Avalonia.Media;
using DynamicData;
using Material.Icons;
using ReactiveUI;

namespace Asv.Drones.Gui.Uav;

public class AdsbAnchor : AdsbAnchorBase
{
    private readonly ILocalizationService _loc;
    
    public AdsbAnchor(IAdsbClientDevice device, ILocalizationService loc) : base(device, "adsb")
    {
        _loc = loc;
        Size = 48;
        OffsetX = OffsetXEnum.Center;
        OffsetY = OffsetYEnum.Center;
        StrokeThickness = 1;
        Stroke = Brushes.SeaGreen;
        IconBrush = Brushes.Teal;
        IsVisible = true;
        Icon = MaterialIconKind.Plane;
        
        // TODO: create device subscriptions
        device.Name.Subscribe(_ => Title = _).DisposeItWith(Disposable);
        device.Adsb.OnTarget.ObserveOn(RxApp.MainThreadScheduler).Subscribe(_ =>
            {
                Location = new GeoPoint(_.Lat, _.Lon, _.Altitude);
            })
            .DisposeItWith(Disposable);
        
    }
}