using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Asv.Avalonia;
using Asv.Avalonia.GeoMap;
using Asv.Avalonia.IO;
using Asv.Avalonia.Plugins;
using Asv.Common;
using Asv.Drones.Api;
using Avalonia;
using Avalonia.Controls;
using DotNext;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Drones.Desktop;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args, ShutdownMode.OnMainWindowClose);
            AppHost.Instance.StopAsync().GetAwaiter().GetResult();
            Task.Run(AppHost.Instance.Dispose).GetAwaiter().GetResult();
        }
        catch (Exception e)
        {
            AppHost.HandleApplicationCrash(e);
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder
            .Configure<App>()
            .UsePlatformDetect()
            .With(new Win32PlatformOptions { OverlayPopups = true }) // Windows
            .With(new X11PlatformOptions { OverlayPopups = true, UseDBusFilePicker = false }) // Unix/Linux
            .With(new AvaloniaNativePlatformOptions { OverlayPopups = true }) // Mac
            .WithInterFont()
            .LogToTrace()
            .UseAsv(builder =>
            {
                builder
                    .UseDefault()
                    .UseAppInfo(configure => configure.FillFromAssembly(typeof(App).Assembly))
                    .UseOptionalLogToFile()
                    .UseOptionalLogViewer()
                    .UseOptionalSoloRun(opt => opt.WithArgumentForwarding())
                    .UsePluginManager(options =>
                    {
                        options.WithApiPackage(typeof(MavlinkHost).Assembly);
                        options.WithPluginPrefix("Asv.Drones.Plugin.");
                    })
                    .UseDesktopShell()
                    .UseModulePlugins(configure =>
                    {
                        configure
                            .WithApiPackage(typeof(MavlinkHost).Assembly)
                            .UseOptionalInstalled() // register installed plugins page
                            .UseOptionalMarket(); // register market plugins page
                    })
                    .UseModuleGeoMap()
                    .UseModuleIo()
                    .UseDronesApp();
            });
    }
}
