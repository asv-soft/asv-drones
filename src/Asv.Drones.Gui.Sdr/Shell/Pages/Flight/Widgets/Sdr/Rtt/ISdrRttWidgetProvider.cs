using Asv.Mavlink;
using Asv.Mavlink.V2.AsvSdr;

namespace Asv.Drones.Gui.Sdr;

public interface ISdrRttWidgetProvider
{
    public  ISdrRttWidget Create(ISdrClientDevice device, AsvSdrCustomMode mode);
}