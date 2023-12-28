using Asv.Common;

namespace Asv.Avalonia.Map
{
    /// <summary>
    /// Represents an interface for a map control.
    /// </summary>
    public interface Interface
    {
        /// <summary>
        /// Gets or sets the geographical position.
        /// </summary>
        GeoPoint Position
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the position of the object in pixels.
        /// </summary>
        /// <value>
        /// A <see cref="GPoint"/> representing the position in pixels.
        /// </value>
        GPoint PositionPixel
        {
            get;
        }

        /// <summary>
        /// Gets or sets the location of the cache.
        /// </summary>
        /// <value>
        /// A string representing the location of the cache.
        /// </value>
        string CacheLocation
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether the object is currently being dragged.
        /// </summary>
        /// <value>
        /// <c>true</c> if the object is currently being dragged; otherwise, <c>false</c>.
        /// </value>
        bool IsDragging
        {
            get;
        }

        /// <summary>
        /// Gets the rectangular latitude and longitude coordinates that make up the view area.
        /// </summary>
        /// <remarks>
        /// This property represents the geographical area visible on the map control.
        /// It is represented by a RectLatLng object, which contains the latitude and longitude
        /// values of the top-left and bottom-right corners of the view area rectangle.
        /// </remarks>
        /// <returns>
        /// The RectLatLng object representing the view area coordinates.
        /// </returns>
        RectLatLng ViewArea
        {
            get;
        }

        /// <summary>
        /// Gets or sets the map provider for the map.
        /// </summary>
        /// <value>
        /// The map provider.
        /// </value>
        GMapProvider MapProvider
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the map can be dragged or not.
        /// </summary>
        /// <value><c>true</c> if the map can be dragged; otherwise, <c>false</c>.</value>
        bool CanDragMap
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the render mode of the property.
        /// </summary>
        /// <returns>
        /// The render mode of the property.
        /// </returns>
        RenderMode RenderMode
        {
            get;
        }

        // events
        /// <summary>
        /// Event raised when the position has changed.
        /// </summary>
        event PositionChanged OnPositionChanged;

        /// <summary>
        /// Event handler that is triggered when a tile finishes loading.
        /// </summary>
        /// <remarks>
        /// This event is raised after a tile is successfully loaded and is ready for use.
        /// Attach your custom event handler to this event to perform any additional tasks
        /// or update the user interface based on the loaded tile data.
        /// </remarks>
        event TileLoadComplete OnTileLoadComplete;

        /// <summary>
        /// Event fired when the loading of a tile begins.
        /// </summary>
        event TileLoadStart OnTileLoadStart;

        /// <summary>
        /// Event that is raised when the map is dragged.
        /// </summary>
        event MapDrag OnMapDrag;

        /// <summary>
        /// Event that is triggered when the map zoom is changed.
        /// </summary>
        event MapZoomChanged OnMapZoomChanged;

        /// <summary>
        /// Event that is triggered when the map's type changes.
        /// </summary>
        /// <remarks>
        /// Use this event to handle changes in the map's type. For example,
        /// you can change the styling or behavior of the map based on its type.
        /// </remarks>
        event MapTypeChanged OnMapTypeChanged;

        /// <summary>
        /// Reloads the map.
        /// </summary>
        void ReloadMap();

        /// <summary>
        /// Converts the coordinates of a point from local x and y to latitude and longitude coordinates.
        /// </summary>
        /// <param name="x">The x-coordinate of the point in local coordinates.</param>
        /// <param name="y">The y-coordinate of the point in local coordinates.</param>
        /// <returns>A GeoPoint object representing the latitude and longitude coordinates of the point.</returns>
        GeoPoint FromLocalToLatLng(int x, int y);

        /// <summary>
        /// Converts a geographical point to a local point.
        /// </summary>
        /// <param name="point">The geographical point to convert.</param>
        /// <returns>The local point.</returns>
        /// <remarks>This method takes a geographical point and converts it to a corresponding local point using a specific mapping algorithm.</remarks>
        GPoint FromLatLngToLocal(GeoPoint point);

#if SQLite
        bool ShowExportDialog();
        bool ShowImportDialog();
#endif
    }
}
