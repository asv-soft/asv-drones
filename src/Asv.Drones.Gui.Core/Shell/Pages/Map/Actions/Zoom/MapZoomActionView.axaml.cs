using System.ComponentModel.Composition;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Core;

[ExportView(typeof(MapZoomActionViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class MapZoomActionView : ReactiveUserControl<MapZoomActionViewModel>
{
    public MapZoomActionView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}