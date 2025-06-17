using Asv.Avalonia.GeoMap;
using Asv.Common;
using Avalonia.Media;
using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Drones;

public class MissionAnchor : MapAnchor<MissionAnchor>
{
    public MissionAnchor(int index, GeoPoint current, GeoPoint next, ILoggerFactory loggerFactory)
        : base($"wayPoint{index}", loggerFactory) // TODO: Use a more descriptive ID with drone ID
    {
        Location = current;
        Title = index.ToString();
        IsReadOnly = true;
        IsVisible = true;
        Icon = MaterialIconKind.MapMarker;
        CenterY = new VerticalOffset(VerticalOffsetEnum.Bottom, 0);
        Foreground = Brushes.Red;
        Polygon.Add(current);
        Polygon.Add(next);
    }

    public MissionAnchor(int index, GeoPoint current, ILoggerFactory loggerFactory)
        : base($"wayPoint{index}", loggerFactory)
    {
        Location = current;
        Title = index.ToString();
        IsReadOnly = true;
        IsVisible = true;
        Icon = MaterialIconKind.MapMarker;
        CenterY = new VerticalOffset(VerticalOffsetEnum.Bottom, 0);
        Foreground = Brushes.Red;
    }
}
