namespace Asv.Drones.Gui.Core
{
    /// <summary>
    /// Represents the main view interface of the application.
    /// </summary>
    public interface IShell
    {
        /// <summary>
        /// Gets or sets the currently active shell page.
        /// </summary>
        /// <value>
        /// The currently active shell page. Can be null if no shell page is currently active.
        /// </value>
        IShellPage? CurrentPage { get; set; }
    }
}