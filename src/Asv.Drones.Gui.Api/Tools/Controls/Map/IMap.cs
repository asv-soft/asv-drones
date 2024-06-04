using System.Collections.ObjectModel;
using System.ComponentModel;
using Asv.Common;
using DynamicData;

namespace Asv.Drones.Gui.Api
{
    /// <summary>
    /// Provides an interface that dictates the structure and behavior of a Map.
    /// The Map is designed to be interactive and allows for various manipulations.
    /// </summary>
    public interface IMap : INotifyPropertyChanged
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
        /// Gets a read-only collection of anchors on the map.
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

        ISourceCache<IMapAnchor, Uri> AdditionalAnchorsSource { get; }
    }
}