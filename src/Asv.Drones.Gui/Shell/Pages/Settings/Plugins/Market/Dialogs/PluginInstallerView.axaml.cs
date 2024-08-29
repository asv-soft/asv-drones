using Asv.Drones.Gui.Api;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui;

[ExportView(typeof(PluginInstallerViewModel))]
public partial class PluginInstallerView : ReactiveUserControl<PluginInstallerViewModel>
{
    public PluginInstallerView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
