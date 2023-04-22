using Asv.Mavlink;

namespace Asv.Drones.Gui.Sdr;

public interface ISdrRttItemProvider
{
    public IEnumerable<ISdrRttItem> Create(ISdrClientDevice device);
}