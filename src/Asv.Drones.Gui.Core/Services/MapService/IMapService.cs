using Asv.Avalonia.Map;
using Asv.Common;

namespace Asv.Drones.Gui.Core
{
    /// <summary>
    /// Represents the configuration settings for a map service.
    /// </summary>
    public class MapServiceConfig
    {
        /// <summary>
        /// Gets or sets the name of the map provider.
        /// </summary>
        /// <remarks>
        /// This property represents the name of the map provider used for displaying maps.
        /// It can be used to identify the specific map provider being used, for example, "Google Maps" or "Bing Maps".
        /// </remarks>
        /// <value>
        /// The name of the map provider.
        /// </value>
        public string? MapProviderName { get; set; }
    }

    /// <summary>
    /// Represents a map service that can calculate map cache size, set map cache directory, and provide information about the current map provider and available providers.
    /// </summary>
    public interface IMapService
    {
        /// <summary>
        /// Calculates the size of the map cache.
        /// </summary>
        /// <returns>
        /// The size of the map cache in bytes.
        /// </returns>
        long CalculateMapCacheSize();

        /// <summary>
        /// Sets the directory where map cache will be stored.
        /// </summary>
        /// <param name="path">The path to the directory.</param>
        void SetMapCacheDirectory(string path);

        /// <summary>
        /// Gets the directory path where the cache for the map is stored.
        /// </summary>
        /// <returns>The directory path where the cache for the map is stored.</returns>
        string MapCacheDirectory { get; }

        /// <summary>
        /// Gets or sets the current map provider that is used for displaying the map.
        /// </summary>
        /// <remarks>
        /// The <see cref="CurrentMapProvider"/> property represents an <see cref="IRxEditableValue"/> interface
        /// for the selected map provider. It allows getting or setting the currently used <see cref="GMapProvider"/>,
        /// which is responsible for rendering the map.
        /// </remarks>
        /// <value>
        /// An implementation of the <see cref="IRxEditableValue"/> interface with the underlying type <see cref="GMapProvider"/>.
        /// </value>
        IRxEditableValue<GMapProvider> CurrentMapProvider { get; }

        /// <summary>
        /// Gets the available providers for GMap.
        /// </summary>
        /// <returns>
        /// An enumerable collection of GMapProvider objects representing the available providers for GMap.
        /// </returns>
        IEnumerable<GMapProvider> AvailableProviders { get; }
    }
}