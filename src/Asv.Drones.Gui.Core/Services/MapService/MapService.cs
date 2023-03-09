using System.ComponentModel.Composition;
using System.Reactive.Linq;
using Asv.Avalonia.Map;
using Asv.Cfg;
using Asv.Common;

namespace Asv.Drones.Gui.Core
{
    [Export(typeof(IMapService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class MapService: ServiceWithConfigBase<MapServiceConfig>, IMapService
    {
        private readonly IRxEditableValue<GMapProvider> _currentMapProvider;
        private string _mapCacheDirectory;

        [ImportingConstructor]
        public MapService(IConfiguration cfgService,IAppPathInfo paths) : base(cfgService)
        {
            if (cfgService == null) throw new ArgumentNullException(nameof(cfgService));

            // this is for store map tiles
            Cache.CacheFolder = Path.Combine(paths.ApplicationDataFolder, "map");
            if (Directory.Exists(Cache.CacheFolder) == false)
            {
                Directory.CreateDirectory(Cache.CacheFolder);
            }

            var providerName = InternalGetConfig(_ => _.MapProviderName);
            if (providerName.IsNullOrWhiteSpace())
            {
                InternalSaveConfig(_=>_.MapProviderName = BingHybridMapProvider.Instance.Name);
                providerName = BingHybridMapProvider.Instance.Name;
            }
            var provider = GMapProviders.List.FirstOrDefault(_ => _.Name == providerName);
            if (provider == null)
            {
                InternalSaveConfig(_ => _.MapProviderName = BingHybridMapProvider.Instance.Name);
                provider = BingHybridMapProvider.Instance;
            }
            _currentMapProvider = new RxValue<GMapProvider>(provider).DisposeItWith(Disposable);
            _currentMapProvider
                .DistinctUntilChanged()
                .Skip(1)
                .Subscribe(_ => InternalSaveConfig(cfg => cfg.MapProviderName = _.Name))
                .DisposeItWith(Disposable);
        }

        public long CalculateMapCacheSize()
        {
            return DirSize(new DirectoryInfo(Cache.CacheFolder));
        }

        private static long DirSize(DirectoryInfo d)
        {
            // if folder not exist return zero size
            if (d.Exists == false) return 0;
            // Add file sizes.
            var fis = d.GetFiles();
            var size = fis.Sum(fi => fi.Length);
            // Add subdirectory sizes.
            var dis = d.GetDirectories();
            size += dis.Sum(DirSize);
            return size;
        }

        public void SetMapCacheDirectory(string directory)
        {
            _mapCacheDirectory = directory;
        }

        public string MapCacheDirectory => _mapCacheDirectory;

        public IRxEditableValue<GMapProvider> CurrentMapProvider => _currentMapProvider;

        public IEnumerable<GMapProvider> AvailableProviders => GMapProviders.List;

    }
}