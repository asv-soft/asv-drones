using System.ComponentModel.Composition;
using Asv.Cfg;
using Asv.Common;
using Asv.Drones.Core;
using Avalonia.Platform.Storage;
using LiteDB;
using NLog;

namespace Asv.Drones.Gui.Core
{
    public class AppServiceConfig
    {
        public string LastAppStorePath { get; set; }
    }

    [Export(typeof(IAppService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class AppService :ServiceWithConfigBase<AppServiceConfig>, IAppService
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly LiteDbAppStore _store;

        [ImportingConstructor]
        public AppService(IConfiguration cfg, IAppPathInfo path, IAppInfo info):base(cfg)
        {
            Paths = path;
            Info = info;

            _store = new LiteDbAppStore(new LiteDatabase(new ConnectionString(path.StoreFilePath)
            {
                Connection = ConnectionType.Direct,
            }), path.StoreFilePath).DisposeItWith(Disposable);
        }

        public IAppInfo Info { get; }
        public IAppPathInfo Paths { get; }
        public IAppStore Store => _store;
    }
}