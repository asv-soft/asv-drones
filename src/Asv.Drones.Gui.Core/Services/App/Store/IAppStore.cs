using LiteDB;

namespace Asv.Drones.Core
{
    /// <summary>
    /// Represents an interface for an app store.
    /// </summary>
    public interface IAppStore:IDisposable
    {
        /// <summary>
        /// Retrieves the file size in bytes.
        /// </summary>
        /// <returns>The file size in bytes.</returns>
        int GetFileSizeInBytes();

        /// <summary>
        /// Gets the instance of the LiteDatabase for the property.
        /// </summary>
        /// <remarks>
        /// The LiteDatabase is responsible for managing the connection to the database and provides methods for executing database operations.
        /// </remarks>
        /// <value>
        /// An instance of the LiteDatabase.
        /// </value>
        ILiteDatabase Db { get; }

        /// <summary>
        /// Gets the name of the source.
        /// </summary>
        /// <value>
        /// The name of the source.
        /// </value>
        string SourceName { get; }
    }

    
}