using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Api;

[ExportView(typeof(MapZoomActionViewModel))]
public partial class MapZoomActionView : ReactiveUserControl<MapZoomActionViewModel>
{
    public MapZoomActionView()
    {
        InitializeComponent();
    }
}