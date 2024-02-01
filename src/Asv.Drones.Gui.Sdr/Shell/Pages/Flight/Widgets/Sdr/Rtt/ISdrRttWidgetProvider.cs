using Asv.Mavlink;
using Asv.Mavlink.V2.AsvSdr;

namespace Asv.Drones.Gui.Sdr;

/// <summary>
/// Provides an interface for creating an SdrRttWidget.
/// </summary>
public interface ISdrRttWidgetProvider
{
    /// <summary>
    /// Creates a SDR RTT widget with the specified device and custom mode.
    /// </summary>
    /// <param name="device">The SDR client device.</param>
    /// <param name="mode">The custom mode for the SDR RTT widget.</param>
    /// <returns>An instance of ISdrRttWidget.</returns>
    public ISdrRttWidget Create(ISdrClientDevice device, AsvSdrCustomMode mode);
}