using Asv.Drones.Core;

namespace Asv.Drones.Gui.Core
{


    public interface IAppService
    {
        /// <summary>
        /// Name, version and other info about application
        /// </summary>
        IAppInfo Info { get; }
        /// <summary>
        /// Default application paths
        /// </summary>
        IAppPathInfo Paths { get; }
        /// <summary>
        /// Application store database, where all modules and plugins can store data
        /// </summary>
        IAppStore Store { get; }
    }

    
}