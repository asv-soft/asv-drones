using Asv.Drones.Gui.Api;

namespace Asv.Drones.Gui;

/// <summary>
/// Represents a User-Accessible Visual Real-Time Tracking (UAV-RTT) item.
/// </summary>
/// <remarks>
/// This interface extends the IViewModel interface.
/// </remarks>
public interface IUavRttItem : IViewModel
{
    /// <summary>
    /// Gets the order number. </summary> <value>
    /// The order number. </value>
    /// /
    int Order { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the property is visible.
    /// </summary>
    /// <value>
    /// <c>true</c> if the property is visible; otherwise, <c>false</c>.
    /// </value>
    bool IsVisible { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the window should be minimized when visible. </summary>
    /// <value>
    /// <c>true</c> if the window should be minimized when visible; otherwise, <c>false</c>. </value>
    /// /
    bool IsMinimizedVisible { get; set; }
}