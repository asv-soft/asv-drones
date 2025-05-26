using Asv.Avalonia.Map;
using Asv.Common;
using Avalonia.Media;
using Material.Icons;

namespace Asv.Drones;

public class MissionAnchor : MapAnchor<MissionAnchor>
{
    public MissionAnchor(int index, GeoPoint current, GeoPoint next)
        : base($"wayPoint{index}")
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

    public MissionAnchor(int index, GeoPoint current)
        : base($"wayPoint{index}")
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
