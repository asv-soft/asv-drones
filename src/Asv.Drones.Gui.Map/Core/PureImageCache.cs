namespace Asv.Avalonia.Map
{
    /// <summary>
    /// Pure abstraction for image cache.
    /// Provides the functionality to store and retrieve images from a cache.
    /// </summary>
    public interface PureImageCache
    {
        /// <summary>
        /// Puts an image to the cache.
        /// </summary>
        /// <param name="tile">The image to be stored in the cache as a byte array.</param>
        /// <param name="type">The type of the image.</param>
        /// <param name="pos">The position of the image.</param>
        /// <param name="zoom">The zoom level of the image.</param>
        /// <returns>True if the image was successfully stored in the cache; otherwise, false.</returns>
        bool PutImageToCache(byte[] tile, int type, GPoint pos, int zoom);

        /// <summary>
        /// Gets the image from the cache based on the specified parameters.
        /// </summary>
        /// <param name="type">The type of image to retrieve from the cache.</param>
        /// <param name="pos">The position of the image.</param>
        /// <param name="zoom">The zoom level of the image.</param>
        /// <returns>The requested PureImage from the cache.</returns>
        PureImage GetImageFromCache(int type, GPoint pos, int zoom);

        /// <summary>
        /// Deletes old tiles older than the specified date from the specified provider or all providers.
        /// </summary>
        /// <param name="date">The date. Tiles older than this will be deleted.</param>
        /// <param name="type">The provider dbid or null to use all providers.</param>
        /// <returns>The number of deleted tiles.</returns>
        int DeleteOlderThan(DateTime date, int? type);
    }
}
