using Avalonia.Controls;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Api;

public class MapActionBase : ViewModelBase, IMapAction
{
    protected MapActionBase(Uri id) : base(id)
    {
    }

    protected MapActionBase(string id) : base(id)
    {
    }

    [Reactive] public Dock Dock { get; set; } = Dock.Right;
    public int Order { get; set; }
    public IMap? Map { get; private set; }

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