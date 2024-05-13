namespace Asv.Drones.Gui.Api;

public interface IMapStatusItem : IShellStatusItem
{
    IShellStatusItem Init(IMap context);
}

public abstract class MapStatusItem : ShellStatusItem, IMapStatusItem
{
    protected MapStatusItem(Uri id) : base(id)
    {
    }

    protected MapStatusItem(string id) : base(id)
    {
    }

    public IShellStatusItem Init(IMap context)
    {
        MapContext = context;
        return this;
    }

    protected IMap MapContext { get; set; }
}