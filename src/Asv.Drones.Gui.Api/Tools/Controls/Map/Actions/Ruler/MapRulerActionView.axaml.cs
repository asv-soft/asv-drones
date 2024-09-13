using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Api;

[ExportView(typeof(MapRulerActionViewModel))]
public partial class MapRulerActionView : ReactiveUserControl<MapRulerActionViewModel>
{
    public MapRulerActionView()
    {
        InitializeComponent();
    }
}