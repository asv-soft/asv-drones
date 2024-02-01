using Asv.Drones.Core;

namespace Asv.Drones.Gui.Core
{
    /// <summary>
    /// Represents an application service that provides information, paths, and a store for the application.
    /// </summary>
    public interface IAppService
    {
        /// <summary>
        /// Gets the name, version, and other information about the application.
        /// </summary>
        IAppInfo Info { get; }

        /// <summary>
        /// Gets the default application paths.
        /// </summary>
        /// <value>
        /// The default application paths.
        /// </value>
        IAppPathInfo Paths { get; }

        /// <summary>
        /// Gets the application store database, where all modules and plugins can store data.
        /// </summary>
        IAppStore Store { get; }
    }
}