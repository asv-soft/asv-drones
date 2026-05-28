using System;
using Asv.Avalonia.Launcher;
using Avalonia.Markup.Xaml;

namespace Asv.Drones.Launcher;

public partial class App : LauncherApp
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    protected override void ConfigureLauncher(LauncherApplicationOptions options)
    {
        options.IconSource = new Uri("avares://Asv.Drones.Launcher/Assets/app.ico");
        options.Title = "Launcher for Asv.Drones";
        options.Description = "Asv.Drones startup launcher";
        options.Footer = "made by ASV.SOFT team";
    }
}
