using Asv.Cfg;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Reactive.Concurrency;
using Asv.Drones.Gui.Uav;
using NLog;
using ReactiveUI;

namespace Asv.Drones.Gui.Core
{
    /// <summary>
    /// This module only imports all core services to be created by IoC container
    /// </summary>
    [PluginEntryPoint(Name, CorePlugin.Name)]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class CoreServicePlugin:IPluginEntryPoint, IObserver<Exception>
    {
        public const string Name = "CoreServices";
        private readonly CompositionContainer _container;
        private ILogService? _log;
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        [ImportingConstructor]
        public CoreServicePlugin(CompositionContainer container)
        {
            _container = container;
        }

        public void Initialize()
        {
            
        }

        public void OnFrameworkInitializationCompleted()
        {
            // We need the plugin to ask the container to create services to make them work
            var svc1 = _container.GetExportedValue<IConfiguration>();
            var svc2 = _container.GetExportedValue<IAppService>();
            var svc3 = _container.GetExportedValue<IThemeService>();
            var svc4 = _container.GetExportedValue<INavigationService>();
            var svc5 = _container.GetExportedValue<ILocalizationService>();
            var svc6 = _container.GetExportedValue<IMapService>();
            var svc7 = _container.GetExportedValue<IMavlinkDevicesService>();
            _log = _container.GetExportedValue<ILogService>();
            RxApp.DefaultExceptionHandler = this;
        }

        public void OnShutdownRequested()
        {
            
        }

        public void OnCompleted()
        {
            if (Debugger.IsAttached) Debugger.Break();
            RxApp.MainThreadScheduler.Schedule(() => throw new NotImplementedException());
        }

        public void OnError(Exception error)
        {
            Logger.Error(error,$"Unhandled task exception {error.Message}");
            if (Debugger.IsAttached) Debugger.Break();
            RxApp.MainThreadScheduler.Schedule(() => throw error);
        }

        public void OnNext(Exception value)
        {
            if (Debugger.IsAttached) Debugger.Break();
            Logger.Error(value,$"Unhandled RxApp exception {value.Message}");
            _log?.Error("Core",$"Unhandled RxApp exception:{value.Message}",value);
        }
    }
}