using Asv.Drones.Gui.Core;

namespace Asv.Drones.Gui.Gbs;

/// <summary>
/// Represents an item in the GbsRtt system.
/// </summary>
public interface IGbsRttItem:IViewModel
{
    /// <summary>
    /// Gets the value of the Order property.
    /// </summary>
    /// <returns>
    /// An integer representing the order.
    /// </returns>
    int Order { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the object is visible.
    /// </summary>
    /// <value>
    /// <c>true</c> if the object is visible; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// This property is used to control the visibility of an object. When set to <c>true</c>, the object is visible;
    /// when set to <c>false</c>, the object is hidden.
    /// </remarks>
    bool IsVisible { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the control is minimized and visible.
    /// </summary>
    /// <value>
    /// <c>true</c> if the control is minimized and visible; otherwise, <c>false</c>.
    /// </value>
    bool IsMinimizedVisible { get; set; }
}