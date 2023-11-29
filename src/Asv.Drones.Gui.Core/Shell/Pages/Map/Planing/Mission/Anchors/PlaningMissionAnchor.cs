using Asv.Avalonia.Map;
using Asv.Common;
using Avalonia.Media;
using Material.Icons;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Core;

public class PlaningMissionAnchor : MapAnchorBase
{
    public const string UriString = PlaningPageViewModel.UriString + "/layer/{0}";

    protected PlaningMissionAnchor(PlaningMissionPointModel point) : base(new Uri(UriString.FormatWith($"{point.Index}/{point.Type}")))
    {
        Title = $"{point.Type} {point.Index}";
        Location = point.Location;
        IsFilled = true;
        IsEditable = true;
        IsVisible = true;
        OffsetX = OffsetXEnum.Center;
        OffsetY = OffsetYEnum.Center;
        PathOpacity = 0.6;
        IconBrush = Brushes.Purple;
    }
    
    [Reactive]
    public int Index { get; set; }
}