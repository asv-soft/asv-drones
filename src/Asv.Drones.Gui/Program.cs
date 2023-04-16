using Avalonia;
using Avalonia.ReactiveUI;
using NLog;
using ReactiveUI;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Asv.Drones.Gui
{
    internal class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            TaskScheduler.UnobservedTaskException += (sender, eventArgs) =>
            {
                if (Debugger.IsAttached) Debugger.Break();
                Logger.Fatal(eventArgs.Exception, "Unobserved task exception:{0}", eventArgs.Exception.Message);
            };
            RxApp.DefaultExceptionHandler = new NlogExceptionHandler();

            try
            {
                BuildAvaloniaApp()
                    .StartWithClassicDesktopLifetime(args);
            }
            catch (Exception e)
            {
                if (Debugger.IsAttached) Debugger.Break();
                Logger.Fatal(e, "Unhandled avalonia exception:{0}", e.Message);
            }
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .With(new X11PlatformOptions
                {
                    EnableMultiTouch = true,
                    UseDBusMenu = true,
                    EnableIme = true
                })
                .With(new Win32PlatformOptions
                {
                    // Avalonia 11.0.0-preview1 issue: CornerRadius not clipping,
                    // Avalonia 11.0.0-preview1 issue: sometimes might crash by collection enumerate fail
                    UseCompositor = false,
                    AllowEglInitialization = false,
                    UseDeferredRendering = true,
                })
                .With(new X11PlatformOptions
                {
                    // UseCompositor = false
                })
                .With(new AvaloniaNativePlatformOptions
                {
                    // UseCompositor = false,
                    // UseGpu = true
                })
                .LogToTrace()
                .UseReactiveUI();
    }
}
