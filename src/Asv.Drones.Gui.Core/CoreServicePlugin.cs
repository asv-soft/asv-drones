using Asv.Cfg;
using System.ComponentModel.Composition;

namespace Asv.Drones.Gui.Core
{
    /// <summary>
    /// This module only imports all core services to be created by IoC container
    /// </summary>
    [PluginEntryPoint("CoreServices", LoadingOrder)]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class CoreServicePlugin:IPluginEntryPoint
    {
        public const int LoadingOrder = CorePlugin.LoadingOrder + byte.MaxValue; // load after core plugin

        [ImportingConstructor]
        public CoreServicePlugin(IConfiguration config,
            IAppService appService,
            IThemeService themeService,
            INavigationService navigation,
            ILocalizationService localization,
            ILogService logService)
        {
            
        }

        public void Initialize()
        {
            
        }

        public void OnFrameworkInitializationCompleted()
        {
            
        }

        public void OnShutdownRequested()
        {
            
        }
    }
}