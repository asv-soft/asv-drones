using Asv.Avalonia.Map;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Asv.Mavlink.Vehicle;
using Avalonia.Media;
using DynamicData.Binding;
using Material.Icons;

namespace Asv.Drones.Gui.Uav;

public class AdsbAnchor : MapAnchorBase
{
    private readonly ILocalizationService _loc;
    
    public AdsbAnchor(IAdsbVehicle device, ILocalizationService loc, ushort deviceFullId) 
        : base(new Uri($"adsb/{deviceFullId}/{device.IcaoAddress}"))
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
        device.Location.Subscribe(_=>Location = _).DisposeItWith(Disposable);
    }
}