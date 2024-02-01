using Asv.Mavlink;

namespace Asv.Drones.Gui.Gbs;

/// <summary>
/// Represents a provider for GBS RTT items.
/// </summary>
public interface IGbsRttItemProvider
{
    /// <summary>
    /// Create a collection of GBS RTT items for a given device.
    /// </summary>
    /// <param name="device">The GBS client device.</param>
    /// <returns>
    /// An enumerable collection of GBS RTT items.
    /// </returns>
    public IEnumerable<IGbsRttItem> Create(IGbsClientDevice device);
}