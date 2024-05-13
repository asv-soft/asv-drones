namespace Asv.Drones.Gui.Api
{
    /// <summary>
    /// All status items in shell must implement this interface
    /// </summary>
    public interface IShellStatusItem : IViewModel
    {
        /// <summary>
        /// Display order
        /// </summary>
        int Order { get; }
    }
}