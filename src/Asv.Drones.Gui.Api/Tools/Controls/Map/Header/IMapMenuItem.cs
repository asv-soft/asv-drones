namespace Asv.Drones.Gui.Api;

public interface IMapMenuItem : IMenuItem
{
    IMenuItem Init(IMap context);
}

public class MapMenuItem : MenuItem, IMapMenuItem
{
    public MapMenuItem(Uri id) : base(id)
    {
    }

    public MapMenuItem(string id) : base(id)
    {
    }

    protected IMap MapContext { get; private set; }

    public virtual IMenuItem Init(IMap context)
    {
        MapContext = context;
        return this;
    }
}