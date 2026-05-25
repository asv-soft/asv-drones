using Asv.Avalonia;
using Asv.Avalonia.GeoMap;
using Asv.Common;
using Avalonia.Media;
using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Drones;

public class MissionAnchor : MapAnchor
{
    public MissionAnchor(int index, GeoPoint current, GeoPoint next, ILoggerFactory loggerFactory)
        : base($"wayPoint{index}") // TODO: Use a more descriptive ID with drone ID
    {
        Location = current;
        Header = index.ToString();
        IsReadOnly = true;
        IsVisible = true;
        Icon = MaterialIconKind.MapMarker;
        CenterY = new VerticalOffset(VerticalOffsetEnum.Bottom, 0);
        Polygon.Add(current);
        Polygon.Add(next);
    }

    public MissionAnchor(int index, GeoPoint current, ILoggerFactory loggerFactory)
        : base($"wayPoint{index}")
    {
        Location = current;
        Header = index.ToString();
        IsReadOnly = true;
        IsVisible = true;
        Icon = MaterialIconKind.MapMarker;
        CenterY = new VerticalOffset(VerticalOffsetEnum.Bottom, 0);
    }
}
