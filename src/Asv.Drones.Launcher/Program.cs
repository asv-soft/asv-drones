using Asv.Avalonia.Launcher.Api;
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
            .StartWithClassicDesktopLifetime(GetLauncherArgs(args), ShutdownMode.OnMainWindowClose);

    private static string[] GetLauncherArgs(string[] args)
    {
        if (
            LauncherCommandLineParser.TryParse(args, out var options, out _)
            && options is not null
            && !string.IsNullOrWhiteSpace(options.TargetPath)
        )
        {
            return args;
        }

        var launcherArgs = new string[args.Length + 2];
        for (var i = 0; i < args.Length; i++)
        {
            launcherArgs[i] = args[i];
        }

        launcherArgs[^2] = LauncherCommandLineArguments.TargetArg;
        launcherArgs[^1] = Path.Combine(AppContext.BaseDirectory, DefaultTargetExecutableName);

        return launcherArgs;
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>().UsePlatformDetect()
#if DEBUG

#endif
        .WithInterFont().LogToTrace();
}
