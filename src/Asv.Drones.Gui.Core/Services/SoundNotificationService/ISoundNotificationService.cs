namespace Asv.Drones.Gui.Core;

/// <summary>
/// Interface for a sound notification service.
/// </summary>
public interface ISoundNotificationService
{
    /// <summary>
    /// Sends a notification to the intended recipients.
    /// </summary>
    /// <remarks>
    /// This method is used to notify the intended recipients about a particular event or occurrence.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Example usage:
    /// Notify();
    /// </code>
    /// </example>
    public void Notify();
}