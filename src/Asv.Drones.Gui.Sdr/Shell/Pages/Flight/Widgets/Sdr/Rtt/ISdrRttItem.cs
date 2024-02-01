using Asv.Drones.Gui.Core;

namespace Asv.Drones.Gui.Sdr;

/// <summary>
/// Represents an item in a SdrRtt view model.
/// </summary>
public interface ISdrRttItem:IViewModel
{
    /// <summary>
    /// Gets the order of something.
    /// </summary>
    /// <remarks>
    /// This property represents the order of something. The order can be an integer value that indicates the position or sequence in which something should be processed, displayed, or operated
    /// upon.
    /// </remarks>
    /// <returns>
    /// An integer value representing the order of something.
    /// </returns>
    int Order { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the object is visible.
    /// </summary>
    /// <value>
    /// true if the object is visible; otherwise, false.
    /// </value>
    bool IsVisible { get; set; }
}