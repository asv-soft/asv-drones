using Asv.Common;

namespace Asv.Avalonia.Map
{
    /// <summary>
    /// Interface for providing roads and routes information.
    /// </summary>
    public interface RoadsProvider
    {
        /// <summary>
        /// Calculates the map route using the given list of geo points.
        /// </summary>
        /// <param name="points">The list of geo points representing the route.</param>
        /// <param name="interpolate">True to interpolate the points to create a smooth route, false otherwise.</param>
        /// <returns>
        /// The calculated map route object.
        /// </returns>
        MapRoute GetRoadsRoute(List<GeoPoint> points, bool interpolate);

        /// <summary>
        /// Gets the route for a set of points on the roads.
        /// </summary>
        /// <param name="points">The points on the map in a specific format.</param>
        /// <param name="interpolate">A flag indicating whether to interpolate the route.</param>
        /// <returns>
        /// The calculated route on the roads as a MapRoute object.
        /// </returns>
        /// <remarks>
        /// The points parameter should be formatted as follows: "latitude1,longitude1|latitude2,longitude2|latitude3,longitude3..."
        /// The interpolate parameter determines whether the route should be interpolated, providing additional points along the roads.
        /// </remarks>
        /// <exception cref="ArgumentException">Thrown if the points parameter is null or empty.</exception>
        MapRoute GetRoadsRoute(string points, bool interpolate);
    }
}
