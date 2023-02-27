using Asv.Cfg.Json;
using Asv.Cfg;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using Avalonia.Controls.Templates;
using Avalonia;
using System.Reflection;
using Asv.Avalonia.Map;

namespace Asv.Drones.Gui.Core
{
    [PluginEntryPoint("Core", LoadingOrder)]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class CorePlugin: IPluginEntryPoint
    {
        private readonly IDataTemplateHost _applicationDataTemplateHost;
        private readonly ViewLocator _defaultViewLocator;
        public const int LoadingOrder = int.MinValue; // we need this module to be initialized first

        [ImportingConstructor]
        public CorePlugin(CompositionContainer container, IDataTemplateHost applicationDataTemplateHost)
        {
            _applicationDataTemplateHost = applicationDataTemplateHost;
            var batch = new CompositionBatch();
            var path = GetAppPathInfo();
            var config = new JsonOneFileConfiguration(path.ApplicationConfigFilePath, true, null);
            batch.AddExportedValue(path);
            batch.AddExportedValue<IConfiguration>(config);
            batch.AddExportedValue(_defaultViewLocator = new ViewLocator(container));
            batch.AddExportedValue(GetAppInfo());
            batch.AddExportedValue<ILocalizationService>(new LocalizationServiceBase(config));
            container.Compose(batch);
        }

        public void Initialize()
        {
            _applicationDataTemplateHost.DataTemplates.Add(_defaultViewLocator ?? throw new InvalidOperationException());
        }

        public void OnFrameworkInitializationCompleted()
        {
            // do nothing
        }

        public void OnShutdownRequested()
        {
            _defaultViewLocator.Dispose(); // we need to manually dispose this object (we manually created it in ctor)
        }

        

        

        private IAppPathInfo GetAppPathInfo()
        {
#if DEBUG
            var baseDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); // Path.GetDirectoryName(Environment.ProcessPath) ?? Environment.CurrentDirectory;
#else
            var baseDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
#endif
            var appData = Path.Combine(baseDirectory, "AsvDronesGui");
            var configPath = Path.Combine(appData, "config.json");
            return new AppPathInfo(appData,configPath);
        }

 

        private IAppInfo GetAppInfo()
        {
            var assm = _applicationDataTemplateHost.GetType().Assembly.GetName(); // hack - to get application assembly
            var version = assm.Version?.ToString() ?? "0.0.0";
            var name = assm.Name ?? "Asv.Drones";
            const string author = "https://github.com/asvol";
            const string appUrl = "https://github.com/asvol/asv-drones";
            const string licence = "MIT License";
            var avaloniaVersion = typeof(AppBuilder).Assembly.GetName().Version?.ToString() ?? "0.0.0"; // hack to get Avalonia version
            return new AppInfo(name, version,author, appUrl, licence, avaloniaVersion);
        }
    }
}