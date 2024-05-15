using Asv.Drones.Gui.Api;
using Avalonia.Controls;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui;

[ExportView(typeof(PluginsInstalledViewModel))]
public partial class PluginsInstalledView : ReactiveUserControl<PluginsInstalledViewModel>
{
    public PluginsInstalledView()
    {
        InitializeComponent();
    }
}