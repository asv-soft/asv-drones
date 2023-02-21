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
        [ImportingConstructor]
        public AppService(IConfiguration cfgService, IAppPathInfo defaultPaths, IAppInfo info):base(cfgService)
        {
            Paths = defaultPaths;
            Info = info;
        }

        public IAppInfo Info { get; }
        public IAppPathInfo Paths { get; }
    }
}