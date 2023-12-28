using Asv.Avalonia.Map;
using Asv.Common;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Material.Icons;

namespace Asv.Drones.Gui.Core
{
    /// <summary>
    /// Provides an interface that dictates the structure and behavior of a Map.
    /// The Map is designed to be interactive and allows for various manipulations.
    /// </summary>
    public interface IMap: INotifyPropertyChanged
    {
        /// <summary>
        /// Gets or sets a boolean value indicating whether the map is currently in dialog mode.
        /// </summary>
        bool IsInDialogMode { get; set; }

        /// <summary>
        /// Gets or sets the maximum zoom level allowed on the map.
        /// </summary>
        int MaxZoom { get; set; }

        /// <summary>
        /// Gets or sets the minimum zoom level allowed on the map.
        /// </summary>
        int MinZoom { get; set; }

        /// <summary>
        /// Gets or sets the current zoom level of the map.
        /// </summary>
        double Zoom { get; set; }

        /// <summary>
        /// Gets or sets the center point of the map in geographical coordinates.
        /// </summary>
        GeoPoint Center { get; set; }

        /// <summary>
        /// Gets a read-only collection of markers (points of interest) on the map.
        /// </summary>
        ReadOnlyObservableCollection<IMapAnchor> Markers { get; }

        /// <summary>
        /// Gets or sets the currently selected item (marker) on the map.
        /// </summary>
        IMapAnchor SelectedItem { get; set; }

        /// <summary>
        /// Gets or sets the item (marker) to follow on the map.
        /// </summary>
        IMapAnchor? ItemToFollow { get; set; }

        /// <summary>
        /// Gets or sets a boolean value indicating whether the map is in anchor edit mode.
        /// </summary>
        bool IsInAnchorEditMode { get; set; }

        /// <summary>
        /// Asynchronously displays a target dialog on the map and waits for user response.
        /// </summary>
        /// <param name="text">The text to display on the target dialog.</param>
        /// <param name="cancel">A cancellation token to cancel the operation.</param>
        /// <returns>A task that returns a GeoPoint when completed.</returns>
        Task<GeoPoint> ShowTargetDialog(string text, CancellationToken cancel);
    }

    /// <summary>
    /// Provides an interface that describes the structure and behavior of a Map Anchor.
    /// Map Anchor represents a point of interest on the map.
    /// </summary>
    public interface IMapAnchor : IMapAnchorViewModel, IViewModel
    {
        /// <summary>
        /// Initializes the MapAnchor with a reference to a Map it belongs to.
        /// </summary>
        /// <param name="map">The map the anchor is associated with.</param>
        /// <returns>The initialized MapAnchor.</returns>
        IMapAnchor Init(IMap map);
    }

    /// <summary>
    /// Enumeration to specify the location of a widget on the map.
    /// </summary>
    public enum WidgetLocation
    {
        Left,   
        Right,
        Bottom
    }

    /// <summary>
    /// An interface that describes the structure and behavior of a Map Widget.
    /// Map Widget represents an interface element on the map.
    /// </summary>
    public interface IMapWidget : IViewModel
    {
        /// <summary>
        /// Gets the location of the widget on the map.
        /// </summary>
        WidgetLocation Location { get; }

        /// <summary>
        /// Gets the title of the widget.
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Gets the order of the widget.
        /// </summary>
        int Order { get; }

        /// <summary>
        /// Gets the icon associated with the widget.
        /// </summary>
        MaterialIconKind Icon { get; }

        /// <summary>
        /// Initializes the Map Widget with a reference to a Map it belongs to.
        /// </summary>
        /// <param name="context">The map the widget is associated with.</param>
        /// <returns>The initialized MapWidget.</returns>
        IMapWidget Init(IMap context);
    }
}