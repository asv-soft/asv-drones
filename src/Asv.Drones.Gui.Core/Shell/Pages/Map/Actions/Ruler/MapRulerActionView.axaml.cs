using System.ComponentModel.Composition;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Core;

[ExportView(typeof(MapRulerActionViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class MapRulerActionView : ReactiveUserControl<MapRulerActionViewModel>
{
    public MapRulerActionView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}