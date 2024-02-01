using Asv.Common;

namespace Asv.Avalonia.Map
{
    /// <summary>
    /// DirectionsProvider interface provides methods to get directions between two locations.
    /// </summary>
    public interface DirectionsProvider
    {
        /// <summary>
        /// Get the directions from start location to end location.
        /// </summary>
        /// <param name="direction">An out parameter that will contain the directions object if the method succeeds.</param>
        /// <param name="start">The starting point of the route.</param>
        /// <param name="end">The ending point of the route.</param>
        /// <param name="avoidHighways">A flag indicating whether to avoid highways.</param>
        /// <param name="avoidTolls">A flag indicating whether to avoid tolls.</param>
        /// <param name="walkingMode">A flag indicating whether to enable walking mode.</param>
        /// <param name="sensor">A flag indicating whether to use sensor for directions.</param>
        /// <param name="metric">A flag indicating whether to use metric units for distances.</param>
        /// <returns>
        /// A DirectionsStatusCode indicating the status of the directions request.
        /// </returns>
        DirectionsStatusCode GetDirections(out GDirections direction, GeoPoint start, GeoPoint end,
            bool avoidHighways, bool avoidTolls, bool walkingMode, bool sensor, bool metric);

        /// <summary>
        /// Retrieves the directions between two locations.
        /// </summary>
        /// <param name="direction">The output parameter that will contain the retrieved directions.</param>
        /// <param name="start">The starting location for the directions.</param>
        /// <param name="end">The destination location for the directions.</param>
        /// <param name="avoidHighways">A value indicating whether to avoid highways in the directions.</param>
        /// <param name="avoidTolls">A value indicating whether to avoid toll roads in the directions.</param>
        /// <param name="walkingMode">A value indicating whether to use walking directions.</param>
        /// <param name="sensor">A value indicating whether the request is sent from a sensor device.</param>
        /// <param name="metric">A value indicating whether to use metric units for distances.</param>
        /// <returns>
        /// The status code indicating the result of the directions request.
        /// </returns>
        DirectionsStatusCode GetDirections(out GDirections direction, string start, string end, bool avoidHighways,
            bool avoidTolls, bool walkingMode, bool sensor, bool metric);

        /// <summary>
        /// Retrieves directions between two locations.
        /// </summary>
        /// <param name="status">Out parameter to store the status code of the request.</param>
        /// <param name="start">The starting location for the directions.</param>
        /// <param name="end">The destination location for the directions.</param>
        /// <param name="avoidHighways">Specifies whether to avoid highways in the directions.</param>
        /// <param name="avoidTolls">Specifies whether to avoid toll roads in the directions.</param>
        /// <param name="walkingMode">Specifies whether to request walking directions.</param>
        /// <param name="sensor">Specifies whether the directions service should employ a sensor (such as a GPS receiver) to improve the accuracy of the directions.</param>
        /// <param name="metric">Specifies whether to display distances in metric units.</param>
        /// <returns>An enumerable collection of GDirections objects representing the route alternatives in the response.</returns>
        IEnumerable<GDirections> GetDirections(out DirectionsStatusCode status, string start, string end,
            bool avoidHighways, bool avoidTolls, bool walkingMode, bool sensor, bool metric);

        /// <summary>
        /// Retrieves directions from a start location to an end location.
        /// The service may provide more than one route alternative in the response.
        /// </summary>
        /// <param name="status">An output parameter to store the status code of the directions request.</param>
        /// <param name="start">The starting location for the directions.</param>
        /// <param name="end">The destination location for the directions.</param>
        /// <param name="avoidHighways">A flag indicating whether to avoid highways in the route.</param>
        /// <param name="avoidTolls">A flag indicating whether to avoid toll roads in the route.</param>
        /// <param name="walkingMode">A flag indicating whether to provide walking directions.</param>
        /// <param name="sensor">A flag indicating whether the directions request is from a sensor capable device.</param>
        /// <param name="metric">A flag indicating whether to use metric units for the directions.</param>
        /// <returns>
        /// An enumerable collection of GDirections objects representing different route alternatives.
        /// The status code of the directions request is stored in the 'status' parameter.
        /// </returns>
        IEnumerable<GDirections> GetDirections(out DirectionsStatusCode status, GeoPoint start, GeoPoint end,
            bool avoidHighways, bool avoidTolls, bool walkingMode, bool sensor, bool metric);

        /// <summary>
        /// Retrieves the directions from the start point to the end point using Google Directions API.
        /// </summary>
        /// <param name="direction">An out parameter that will be filled with the retrieved directions.</param>
        /// <param name="start">The starting point for the directions.</param>
        /// <param name="wayPoints">The intermediate waypoints along the route.</param>
        /// <param name="end">The ending point for the directions.</param>
        /// <param name="avoidHighways">Specifies whether to avoid highways in the route (true) or not (false).</param>
        /// <param name="avoidTolls">Specifies whether to avoid tolls in the route (true) or not (false).</param>
        /// <param name="walkingMode">Specifies whether to use walking mode for directions (true) or driving mode (false).</param>
        /// <param name="sensor">Specifies whether the request is from a sensor or not.</param>
        /// <param name="metric">Specifies whether the directions should be returned in metric units (true) or not (false).</param>
        /// <returns>The status code of the directions request.</returns>
        DirectionsStatusCode GetDirections(out GDirections direction, GeoPoint start,
            IEnumerable<GeoPoint> wayPoints, GeoPoint end, bool avoidHighways, bool avoidTolls, bool walkingMode,
            bool sensor, bool metric);

        /// <summary>
        /// Retrieves the directions for a specified route using the Google Directions API.
        /// </summary>
        /// <param name="direction">The resulting directions for the specified route (output parameter).</param>
        /// <param name="start">The starting location of the route.</param>
        /// <param name="wayPoints">The intermediate waypoints of the route.</param>
        /// <param name="end">The destination location of the route.</param>
        /// <param name="avoidHighways">Specifies whether to avoid highways or not.</param>
        /// <param name="avoidTolls">Specifies whether to avoid tolls or not.</param>
        /// <param name="walkingMode">Specifies whether to use walking mode or not.</param>
        /// <param name="sensor">Specifies whether the directions request comes from a device with a sensor or not.</param>
        /// <param name="metric">Specifies whether the distances in the directions response should be returned in metric or imperial units.</param>
        /// <returns>The status code indicating the success or failure of the directions request.</returns>
        DirectionsStatusCode GetDirections(out GDirections direction, string start, IEnumerable<string> wayPoints,
            string end, bool avoidHighways, bool avoidTolls, bool walkingMode, bool sensor, bool metric);
    }
}
