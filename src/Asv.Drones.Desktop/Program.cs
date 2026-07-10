using Asv.Avalonia;
using Asv.Avalonia.Charts;
using Asv.Avalonia.GeoMap;
using Asv.Avalonia.IO;
using Asv.Avalonia.Launcher.Ready;
using Asv.Avalonia.Plugins;
using Asv.Drones.Api;
using Avalonia;
using Avalonia.Controls;
using DotNext;

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
                    .EnableLogging()
                    .RegisterCore(core =>
                    {
                        core.RegisterControls();
                        core.RegisterServices(svc =>
                        {
                            svc.RegisterAppArgsStore();
                            svc.RegisterAppInfo(info =>
                                info.FillFromAssembly(typeof(App).Assembly)
                            );
                            svc.RegisterAppPath();
                            svc.RegisterRestartFeature();
                            svc.RegisterDialogs();
                            svc.RegisterExtensions();
                            svc.RegisterFileAssociation();
                            svc.RegisterUnhandledExceptionsHandler();
                            svc.RegisterHotKeys();
                            svc.RegisterLocalizationService();
                            svc.RegisterLogViewer();
                            svc.RegisterLogToFile();
                            svc.RegisterSearchService();
                            svc.RegisterShellHost();
                            svc.RegisterSoloRun(soloRun => soloRun.WithArgumentForwarding());
                            svc.RegisterThemeService();
                            svc.RegisterTimeProvider();
                            svc.RegisterUnitService();
                            svc.RegisterUserConfig();
                            svc.RegisterViewLocator();
                        });
                    })
                    .RegisterDesktopShell()
                    .RegisterModulePlugins(configure =>
                    {
                        configure.RegisterCore(core =>
                            core.RegisterServices(services =>
                            {
                                services.RegisterPluginBootloader(bootloader =>
                                    bootloader.WithApiPackage(typeof(MavlinkHost).Assembly)
                                );
                                services.RegisterPluginManager(options =>
                                {
                                    options.WithApiPackage(typeof(MavlinkHost).Assembly);
                                    options.WithPluginPrefix("Asv.Drones.Plugin.");
                                });
                            })
                        );
                        configure.RegisterShell();
                    })
                    .RegisterModuleGeoMap()
                    .RegisterModuleCharts()
                    .RegisterModuleIo()
                    .RegisterLauncher(cfg => cfg.IsOptional())
                    .RegisterDronesApp();
            });
    }
}
