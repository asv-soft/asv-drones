using Asv.Avalonia.GeoMap;
using Asv.Common;
using R3;

namespace Asv.Drones;

public static class MapAnchorMixin
{
    extension(IMapAnchor anchor)
    {
        public IDisposable DrawLine(Observable<GeoPoint> first, Observable<GeoPoint> second)
        {
            var oldFirst = GeoPoint.NaN;
            var oldSecond = GeoPoint.NaN;

            var sub1 = first.Subscribe(newPoint =>
                oldFirst = anchor.UpdatePolygon(newPoint, oldFirst)
            );
            var sub2 = second.Subscribe(newPoint =>
                oldSecond = anchor.UpdatePolygon(newPoint, oldSecond)
            );

            return Disposable.Combine(sub1, sub2);
        }

        public GeoPoint UpdatePolygon(GeoPoint newP, GeoPoint oldP)
        {
            if (oldP != GeoPoint.NaN)
            {
                anchor.Polygon.Remove(oldP);
            }

            anchor.Polygon.Add(newP);
            return newP;
        }
    }
}
