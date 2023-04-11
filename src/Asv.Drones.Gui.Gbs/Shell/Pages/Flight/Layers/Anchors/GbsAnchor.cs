using Asv.Avalonia.Map;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink.V2.AsvGbs;
using Avalonia.Media;
using Material.Icons;

namespace Asv.Drones.Gui.Gbs;

public class GbsAnchor : GbsAnchorBase
{
    private readonly ILocalizationService _loc;
    
    public GbsAnchor(IGbsDevice gbs, ILocalizationService loc) : base(gbs, "gbs")
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
        
        gbs.DeviceClient.Position.Subscribe(_ => Location = _).DisposeItWith(Disposable);
        gbs.DeviceClient.Position.Subscribe(_ => UpdateDescription()).DisposeItWith(Disposable);
        gbs.DeviceClient.CustomMode.Subscribe(SetIcon).DisposeItWith(Disposable);
    }

    private void SetIcon(AsvGbsCustomMode mode)
    {
        Icon = mode == AsvGbsCustomMode.AsvGbsCustomModeError ? MaterialIconKind.RouterWirelessOff : MaterialIconKind.RouterWireless;
    }

    private void UpdateDescription()
    {
        Description = string.Format(RS.GbsAnchor_Latitude, _loc.Latitude.FromSiToStringWithUnits(Gbs.DeviceClient.Position.Value.Latitude)) + "\n" +
                      string.Format(RS.GbsAnchor_Longitude, _loc.Longitude.FromSiToStringWithUnits(Gbs.DeviceClient.Position.Value.Longitude)) + "\n" +
                      string.Format(RS.GbsAnchor_GNSS_Altitude, _loc.Altitude.FromSiToStringWithUnits(Gbs.DeviceClient.Position.Value.Altitude)) + "\n";
    }
}