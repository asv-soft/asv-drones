using System.Composition.Hosting;
using Asv.Avalonia;
using Asv.Avalonia.GeoMap;
using Asv.Avalonia.IO;
using Asv.Avalonia.Plugins;
using Asv.Drones.Api;
using Avalonia.Markup.Xaml;

namespace Asv.Drones;

public partial class App : ShellHost
{
    public App()
        : base(cfg =>
        {
            cfg.WithDependenciesFromSystemModule();
            cfg.WithDependenciesFromIoModule();
            cfg.WithDependenciesFromPluginManagerModule();
            cfg.WithDependenciesFromGeoMapModule();
            cfg.WithDependenciesFromApi();
        }) { }

    protected override void LoadXaml()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
