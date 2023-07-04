using Asv.Common;
using Asv.Drones.Core;
using Avalonia.Platform.Storage;

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