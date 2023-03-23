namespace Asv.Drones.Gui.Gbs;

public interface IGbsRttItemProvider
{
    public IEnumerable<IGbsRttItem> Create(IGbsDevice vehicle);
}