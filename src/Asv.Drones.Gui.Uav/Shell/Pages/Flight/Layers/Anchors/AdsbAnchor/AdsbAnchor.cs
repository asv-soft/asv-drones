using Asv.Avalonia.Map;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Avalonia.Media;
using Material.Icons;

namespace Asv.Drones.Gui.Uav;

public class AdsbAnchor : MapAnchorBase
{
    private readonly ILocalizationService _loc;
    public const string UriString = FlightPageViewModel.UriString + "/layer/{0}/{1}";
    
    public AdsbAnchor(IAdsbVehicle device, ILocalizationService loc, ushort deviceFullId) 
        : base(new Uri(FlightPageViewModel.UriString + $"/adsb/{deviceFullId}/{device.IcaoAddress}"))
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
        device.CallSign.Subscribe(_ => Title = _).DisposeItWith(Disposable);
        device.Location.Subscribe(_ => Location = _).DisposeItWith(Disposable);
        device.Heading.Subscribe(_ => RotateAngle = _ - 45).DisposeItWith(Disposable);
    }
}