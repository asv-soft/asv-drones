using Asv.Drones.Gui.Core;

namespace Asv.Drones.Gui.Sdr;

/// <summary>
/// Represents a widget used for real-time tracking and tracing.
/// </summary>
public interface ISdrRttWidget : IViewModel
{
    /// <summary>
    /// Gets the order value of an object.
    /// </summary>
    /// <remarks>
    /// The order value represents the position or sequence of an object within a collection or list.
    /// The lower the order value, the earlier the object should appear in the collection or list.
    /// </remarks>
    /// <value>
    /// An integer representing the order value of the object.
    /// </value>
    int Order { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the object is visible.
    /// </summary>
    /// <value>
    /// <c>true</c> if the object is visible; otherwise, <c>false</c>.
    /// </value>
    bool IsVisible { get; set; }
}