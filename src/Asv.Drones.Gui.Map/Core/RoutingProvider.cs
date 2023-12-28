using Asv.Common;

namespace Asv.Avalonia.Map
{
    /// <summary>
    /// RoutingProvider interface
    /// </summary>
    public interface RoutingProvider
    {
        /// <summary>
        /// Get route between two points.
        /// </summary>
        /// <param name="start">The starting point of the route.</param>
        /// <param name="end">The ending point of the route.</param>
        /// <param name="avoidHighways">A boolean value indicating whether to avoid highways.</param>
        /// <param name="walkingMode">A boolean value indicating whether to use walking mode.</param>
        /// <param name="zoom">The zoom level of the map.</param>
        /// <returns>The MapRoute object representing the route between the two points.</returns>
        MapRoute GetRoute(GeoPoint start, GeoPoint end, bool avoidHighways, bool walkingMode, int zoom);

        /// <summary>
        /// Gets the route between two points on the map.
        /// </summary>
        /// <param name="start">The starting point of the route.</param>
        /// <param name="end">The ending point of the route.</param>
        /// <param name="avoidHighways">Specifies whether to avoid highways when calculating the route.</param>
        /// <param name="walkingMode">Specifies whether to use walking mode when calculating the route.</param>
        /// <param name="zoom">The zoom level of the map.</param>
        /// <returns>The calculated map route.</returns>
        MapRoute GetRoute(string start, string end, bool avoidHighways, bool walkingMode, int zoom);
    }
}
