using Asv.Avalonia.Launcher.Orchestration;
using Avalonia;
using Avalonia.Controls;

namespace Asv.Drones.Launcher;

static class Program
{
    private const string DefaultTargetExecutableName = "Asv.Drones.Desktop.exe";

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) =>
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(
                LauncherCommandLineParser.WithDefaultTarget(args, DefaultTargetExecutableName),
                ShutdownMode.OnMainWindowClose
            );

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>().UsePlatformDetect()
#if DEBUG

#endif
        .WithInterFont().LogToTrace();
}
