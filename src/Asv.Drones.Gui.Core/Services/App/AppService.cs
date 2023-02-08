using System.ComponentModel.Composition;
using Asv.Avalonia.Map;
using Asv.Cfg;

namespace Asv.Drones.Gui.Core
{
    public class AppServiceConfig
    {

    }

    [Export(typeof(IAppService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class AppService :ServiceWithConfigBase<AppServiceConfig>, IAppService
    {
        private readonly IConfiguration _cfgService;

        [ImportingConstructor]
        public AppService(IConfiguration cfgService, IAppPathInfo defaultPaths, IAppInfo info):base(cfgService)
        {
            _cfgService = cfgService;
            Paths = defaultPaths;
            Info = info;
            Cache.CacheFolder = Path.Combine(defaultPaths.ApplicationDataFolder, "map1");
        }

        public IAppInfo Info { get; }
        public IAppPathInfo Paths { get; }
    }
}