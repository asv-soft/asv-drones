using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Api;

[ExportView(typeof(MapMoverActionViewModel))]
public partial class MapMoverActionView : ReactiveUserControl<MapMoverActionViewModel>
{
    public MapMoverActionView()
    {
        InitializeComponent();
    }
}