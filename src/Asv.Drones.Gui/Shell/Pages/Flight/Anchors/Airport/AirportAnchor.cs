using Asv.Avalonia.Map;
using Asv.Drones.Gui.Api;
using Avalonia.Media;
using Material.Icons;

namespace Asv.Drones.Gui.Anchors;

public class AirportAnchor : MapAnchorBase, IAirportAnchor
{
    public AirportAnchor(double id)
        : base($"{WellKnownUri.ShellPageMapFlightAnchor}/airport/{id}")
    {
        Size = 48;
        BaseSize = 48;
        OffsetX = OffsetXEnum.Center;
        OffsetY = OffsetYEnum.Center;
        StrokeThickness = 1;
        BaseStrokeThickness = 1;
        Stroke = Brushes.White;
        IconBrush = Brushes.DodgerBlue;
        IsVisible = true;
        IsEditable = false;
        Icon = MaterialIconKind.Airport;
    }
}