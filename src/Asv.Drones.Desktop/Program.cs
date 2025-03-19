using System;
using Asv.Avalonia;
using Asv.Avalonia.Map;
using Asv.Avalonia.Plugins;
using Asv.Drones.Api;
using Avalonia;
using Avalonia.Controls;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace Asv.Drones.Desktop;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        var builder = AppHost.CreateBuilder(args);

        builder
            .UseAvalonia(BuildAvaloniaApp)
            .UseLogToConsoleOnDebug()
            .UseAppPath(opt => opt.WithRelativeFolder("data"))
            .UseJsonUserConfig(opt =>
                opt.WithFileName("user_settings.json").WithAutoSave(TimeSpan.FromSeconds(1))
            )
            .UseAppInfo(opt => opt.FillFromAssembly(typeof(App).Assembly))
            .UseSoloRun(opt => opt.WithArgumentForwarding())
            .UseLogService(opt => opt.WithRelativeFolder("logs"))
            .UseAsvMap()
            .UsePluginManager(options =>
            {
                options.WithApiPackage(typeof(IFlightMode).Assembly);
                options.WithPluginPrefix("Asv.Drones.Plugin.");
            });
        using var host = builder.Build();
        
        host.StartWithClassicDesktopLifetime(args, ShutdownMode.OnMainWindowClose);
        
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder
            .Configure<App>()
            .UsePlatformDetect()
            .With(new Win32PlatformOptions { OverlayPopups = true }) // Windows
            .With(new X11PlatformOptions { OverlayPopups = true, UseDBusFilePicker = false }) // Unix/Linux
            .With(new AvaloniaNativePlatformOptions { OverlayPopups = true }) // Mac
            .WithInterFont()
            .LogToTrace()
            .UseR3();
}