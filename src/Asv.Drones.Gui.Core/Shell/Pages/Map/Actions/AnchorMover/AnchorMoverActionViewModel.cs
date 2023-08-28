using System.ComponentModel.Composition;
using Avalonia.Controls;

namespace Asv.Drones.Gui.Core;

[Export(FlightPageViewModel.UriString,typeof(IMapAction))]
[Export(PlaningPageViewModel.UriString,typeof(IMapAction))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class AnchorMoverActionViewModel : ViewModelBase, IMapAction
{
    private IMap _map;
    
    public AnchorMoverActionViewModel() : base("asv:shell.page.map.action.move-anchors")
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