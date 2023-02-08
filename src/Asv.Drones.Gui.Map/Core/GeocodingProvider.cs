using Asv.Common;

namespace Asv.Avalonia.Map
{
    /// <summary>
    ///     geocoding interface
    /// </summary>
    public interface GeocodingProvider
    {
        GeoCoderStatusCode GetPoints(string keywords, out List<GeoPoint> pointList);

        GeoPoint? GetPoint(string keywords, out GeoCoderStatusCode status);


        GeoCoderStatusCode GetPoints(Placemark placemark, out List<GeoPoint> pointList);

        GeoPoint? GetPoint(Placemark placemark, out GeoCoderStatusCode status);


        GeoCoderStatusCode GetPlacemarks(GeoPoint location, out List<Placemark> placemarkList);

        Placemark? GetPlacemark(GeoPoint location, out GeoCoderStatusCode status);
    }
}
