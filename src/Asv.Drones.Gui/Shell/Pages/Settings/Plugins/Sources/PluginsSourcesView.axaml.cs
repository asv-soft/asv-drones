using Asv.Drones.Gui.Api;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui;

[ExportView(typeof(PluginsSourcesViewModel))]
public partial class PluginsSourcesView : ReactiveUserControl<PluginsSourcesViewModel>
{
    public PluginsSourcesView()
    {
        InitializeComponent();
    }
}