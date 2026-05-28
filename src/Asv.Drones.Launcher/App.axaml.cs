using Asv.Avalonia.Launcher;
using Avalonia.Markup.Xaml;
using Material.Icons;

namespace Asv.Drones.Launcher;

public partial class App : LauncherApp
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    protected override void ConfigureLauncher(LauncherApplicationOptions options)
    {
        options.IconKind = MaterialIconKind.Drone;
        options.Title = "Launcher for Asv.Drones";
        options.Description = "Asv.Drones startup launcher";
        options.Footer = "made by ASV.SOFT team";
    }
}
