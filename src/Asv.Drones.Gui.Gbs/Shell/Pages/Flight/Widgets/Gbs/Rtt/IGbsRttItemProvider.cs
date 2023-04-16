using Asv.Mavlink;

namespace Asv.Drones.Gui.Gbs;

public interface IGbsRttItemProvider
{
    public IEnumerable<IGbsRttItem> Create(IGbsClientDevice device);
}