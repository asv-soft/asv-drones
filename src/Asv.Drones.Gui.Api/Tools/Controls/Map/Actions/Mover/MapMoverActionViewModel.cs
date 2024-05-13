using Avalonia.Controls;

namespace Asv.Drones.Gui.Api;

public class MapMoverActionViewModel : ViewModelBase, IMapAction
{
    private IMap _map;

    public MapMoverActionViewModel() : base(WellKnownUri.UndefinedUri)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    protected MapMoverActionViewModel(Uri id) : base(id)
    {
    }

    protected MapMoverActionViewModel(string id) : base(id)
    {
    }

    public IMapAction Init(IMap context)
    {
        _map = context;
        return this;
    }

    public IMap Map => _map;
    public Dock Dock { get; }
    public int Order => 0;
}