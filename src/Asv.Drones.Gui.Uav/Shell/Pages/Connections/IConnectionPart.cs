using Asv.Drones.Gui.Core;

namespace Asv.Drones.Gui.Uav
{
    
    /// <summary>
    /// All parts of the connection settings must implement this interface to be displayed in the application
    /// </summary>
    public interface IConnectionPart : IViewModel
    {
        /// <summary>
        /// Part of the connection settings can set it to true to force the application to restart
        /// </summary>
        bool IsRebootRequired { get; }
        /// <summary>
        /// Display order of the part
        /// </summary>
        int Order { get; }
    }
}