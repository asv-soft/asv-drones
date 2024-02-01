namespace Asv.Drones.Gui.Core
{
    /// <summary>
    /// Represents a settings part in a view model.
    /// </summary>
    public interface ISettingsPart : IViewModel
    {
        /// <summary>
        /// Gets the value of the Order property.
        /// </summary>
        /// <returns>An integer representing the order.</returns>
        int Order { get; }

        /// <summary>
        /// Gets or sets a value indicating whether a reboot is required.
        /// </summary>
        /// <value>
        /// <c>true</c> if a reboot is required; otherwise, <c>false</c>.
        /// </value>
        bool IsRebootRequired { get; }
    }
}