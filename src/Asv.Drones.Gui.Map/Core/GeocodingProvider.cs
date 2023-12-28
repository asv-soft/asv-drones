using Asv.Common;

namespace Asv.Avalonia.Map
{
    /// <summary>
    /// Geocoding interface to provide geocoding and reverse geocoding functionality.
    /// </summary>
    public interface GeocodingProvider
    {
        /// <summary>
        /// Gets the geographic points associated with the specified keywords.
        /// </summary>
        /// <param name="keywords">The keywords used to search for geo points.</param>
        /// <param name="pointList">The list of geographic points found.</param>
        /// <returns>The status code indicating the success or failure of the operation.</returns>
        /// <remarks>
        /// This method searches for geographic points based on the provided keywords. It populates the
        /// <paramref name="pointList"/> with any matching geo points found. The status code returned
        /// indicates the success or failure of the operation.
        /// </remarks>
        /// <example>
        /// <code>
        /// List<GeoPoint> points;
        /// GeoCoderStatusCode statusCode = GetPoints("Some keywords", out points);
        /// if (statusCode == GeoCoderStatusCode.Success)
        /// {
        /// // Do something with the points
        /// }
        /// else
        /// {
        /// // Handle the error
        /// }
        /// </code>
        /// </example>
        GeoCoderStatusCode GetPoints(string keywords, out List<GeoPoint> pointList);

        /// <summary>
        /// Retrieves the geographical point for the given keywords.
        /// </summary>
        /// <param name="keywords">The keywords to search for.</param>
        /// <param name="status">The status code indicating the result of the geocoding operation.</param>
        /// <returns>The geographical point for the given keywords, or null if not found.</returns>
        GeoPoint? GetPoint(string keywords, out GeoCoderStatusCode status);


        /// <summary>
        /// Retrieves the points associated with a given <see cref="Placemark"/>.
        /// </summary>
        /// <param name="placemark">The <see cref="Placemark"/> for which points are to be retrieved.</param>
        /// <param name="pointList">The list to store the retrieved <see cref="GeoPoint"/> objects.</param>
        /// <returns>The <see cref="GeoCoderStatusCode"/> indicating the status of the operation.</returns>
        /// <remarks>
        /// This method fetches and populates the <paramref name="pointList"/> with the points associated
        /// with the specified <paramref name="placemark"/>. The method returns a <see cref="GeoCoderStatusCode"/>
        /// indicating the status of the operation. A successful retrieval will return <see cref="GeoCoderStatusCode.Success"/>,
        /// while a failure will return an appropriate error code from the <see cref="GeoCoderStatusCode"/> enumeration.
        /// </remarks>
        GeoCoderStatusCode GetPoints(Placemark placemark, out List<GeoPoint> pointList);

        /// <summary>
        /// Retrieves the geographic coordinates (latitude and longitude) of the given placemark.
        /// </summary>
        /// <param name="placemark">The placemark to retrieve the coordinates for.</param>
        /// <param name="status">Out parameter indicating the status of the geocoding operation.</param>
        /// <returns>
        /// A nullable GeoPoint object representing the geographic coordinates of the placemark.
        /// If the geocoding operation is successful, the coordinates are returned.
        /// If the geocoding operation fails, null is returned.
        /// The status parameter provides additional information about the geocoding operation status.
        /// </returns>
        /// <remarks>
        /// The GetPoint method uses a geocoding service to convert a placemark, such as an address or place name,
        /// into geographic coordinates usable in mapping applications.
        /// The status parameter is an out parameter that indicates the result of the geocoding operation,
        /// providing information such as success, failure, or any error codes encountered.
        /// </remarks>
        GeoPoint? GetPoint(Placemark placemark, out GeoCoderStatusCode status);


        /// <summary>
        /// Retrieves the placemarks for the specified location.
        /// </summary>
        /// <param name="location">The geographical point for which to retrieve placemarks.</param>
        /// <param name="placemarkList">An output parameter to store the retrieved list of placemarks.</param>
        /// <returns>A <see cref="GeoCoderStatusCode"/> indicating the status of the operation.</returns>
        GeoCoderStatusCode GetPlacemarks(GeoPoint location, out List<Placemark> placemarkList);

        /// <summary>
        /// Retrieves a Placemark object corresponding to a given location.
        /// </summary>
        /// <param name="location">The GeoPoint representing the location to retrieve the Placemark for.</param>
        /// <param name="status">An out parameter indicating the status of the geocoding operation.</param>
        /// <returns>
        /// A Placemark object if the geocoding operation is successful; otherwise, null.
        /// </returns>
        /// <remarks>
        /// This method effectively takes a GeoPoint and returns a Placemark object
        /// that contains information about the location, such as its address, latitude,
        /// longitude, and other details. The geocoding operation status can be used
        /// to check if the operation completed successfully or encountered any errors.
        /// </remarks>
        Placemark? GetPlacemark(GeoPoint location, out GeoCoderStatusCode status);
    }
}
