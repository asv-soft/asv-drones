using System.ComponentModel.Composition;
using System.Windows.Input;
using Asv.Common;
using Avalonia.Controls;
using ReactiveUI;

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
        MoveAnchorsToggleCommand = ReactiveCommand.Create(() => _map.IsInAnchorEditMode = IsEnabled)
            .DisposeItWith(Disposable);
        return this;
    }

    public Dock Dock { get; }
    public int Order => 0;
    public ICommand MoveAnchorsToggleCommand { get; set; }
    public bool IsEnabled { get; set; }
}