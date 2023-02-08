using Asv.Common;

namespace Asv.Avalonia.Map
{
    /// <summary>
    ///     routing interface
    /// </summary>
    public interface RoutingProvider
    {
        /// <summary>
        ///     get route between two points
        /// </summary>
        MapRoute GetRoute(GeoPoint start, GeoPoint end, bool avoidHighways, bool walkingMode, int zoom);

        /// <summary>
        ///     get route between two points
        /// </summary>
        MapRoute GetRoute(string start, string end, bool avoidHighways, bool walkingMode, int zoom);
    }
}
