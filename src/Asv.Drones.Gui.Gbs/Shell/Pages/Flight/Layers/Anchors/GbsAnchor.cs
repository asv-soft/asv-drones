using Asv.Avalonia.Map;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Asv.Mavlink.V2.AsvGbs;
using Avalonia.Media;
using Material.Icons;

namespace Asv.Drones.Gui.Gbs;

public class GbsAnchor : GbsAnchorBase
{
    private readonly ILocalizationService _loc;
    
    public GbsAnchor(IGbsClientDevice device, ILocalizationService loc) : base(device, "gbs")
    {
        _loc = loc;
        Size = 48;
        OffsetX = OffsetXEnum.Center;
        OffsetY = OffsetYEnum.Center;
        StrokeThickness = 1;
        Stroke = Brushes.Indigo;
        IconBrush = Brushes.Teal;
        IsVisible = true;
        Title = RS.GbsAnchor_Title;
        
        device.Gbs.Position.Subscribe(_ => Location = _).DisposeItWith(Disposable);
        device.Gbs.Position.Subscribe(_ => UpdateDescription()).DisposeItWith(Disposable);
        device.Gbs.CustomMode.Subscribe(SetIcon).DisposeItWith(Disposable);
    }

    private void SetIcon(AsvGbsCustomMode mode)
    {
        Icon = mode == AsvGbsCustomMode.AsvGbsCustomModeError ? MaterialIconKind.RouterWirelessOff : MaterialIconKind.RouterWireless;
    }

    private void UpdateDescription()
    {
        Description = string.Format(RS.GbsAnchor_Latitude, _loc.Latitude.FromSiToStringWithUnits(Device.Gbs.Position.Value.Latitude)) + "\n" +
                      string.Format(RS.GbsAnchor_Longitude, _loc.Longitude.FromSiToStringWithUnits(Device.Gbs.Position.Value.Longitude)) + "\n" +
                      string.Format(RS.GbsAnchor_GNSS_Altitude, _loc.Altitude.FromSiToStringWithUnits(Device.Gbs.Position.Value.Altitude)) + "\n";
    }
}