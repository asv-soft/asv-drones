using Asv.Mavlink;
using Asv.Mavlink.V2.AsvSdr;

namespace Asv.Drones.Gui.Sdr;

public interface ISdrRttItemProvider
{
    public IEnumerable<ISdrRttItem> Create(ISdrClientDevice device, AsvSdrCustomMode mode);
}