namespace Asv.Drones.Gui.Core;

public class MapActionBase : ViewModelBase, IMapAction
{
    protected MapActionBase(Uri id) : base(id)
    {
    }

    protected MapActionBase(string id) : base(id)
    {
    }

    public int Order { get; }
    protected IMap? Map { get; private set; }
    public virtual IMapAction Init(IMap context)
    {
        Map = context;
        InternalWhenMapLoaded(context);
        return this;
    }

    protected virtual void InternalWhenMapLoaded(IMap context)
    {
        // do nothing            
    }
}