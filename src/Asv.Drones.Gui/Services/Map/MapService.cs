using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using Asv.Avalonia.Map;
using Asv.Cfg;
using Asv.Common;
using Asv.Drones.Gui.Api;

namespace Asv.Drones.Gui
{
    public class MapServiceConfig
    {
        public string? MapProviderName { get; set; }
        public AccessMode MapAccessMode { get; set; } = AccessMode.ServerAndCache;
    }

    [Export(typeof(IMapService))]
    [Shared]
    public class MapService : ServiceWithConfigBase<MapServiceConfig>, IMapService
    {
        private readonly IRxEditableValue<GMapProvider> _currentMapProvider;
        private IRxEditableValue<AccessMode> _currentMapAccessMode;
        private string _mapCacheDirectory;

        [ImportingConstructor]
        public MapService(IConfiguration cfgService, IAppPathInfo paths) : base(cfgService)
        {
            if (cfgService == null) throw new ArgumentNullException(nameof(cfgService));

            // this is for store map tiles
            Cache.CacheFolder = Path.Combine(paths.AppDataFolder, "map");

            GMaps.Instance.BoostCacheEngine = true;
            GMaps.Instance.UseMemoryCache = true;
            GMaps.Instance.CacheOnIdleRead = true;

            if (Directory.Exists(Cache.CacheFolder) == false)
            {
                Directory.CreateDirectory(Cache.CacheFolder);
            }

            var providerName = InternalGetConfig(_ => _.MapProviderName);
            if (providerName.IsNullOrWhiteSpace())
            {
                InternalSaveConfig(_ => _.MapProviderName = BingHybridMapProvider.Instance.Name);
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

            var accessMode = InternalGetConfig(_ => _.MapAccessMode);
            _currentMapAccessMode = new RxValue<AccessMode>(accessMode).DisposeItWith(Disposable);
            _currentMapAccessMode
                .DistinctUntilChanged()
                .Subscribe(_ =>
                {
                    GMaps.Instance.Mode = _;
                    InternalSaveConfig(cfg => cfg.MapAccessMode = _);
                })
                .DisposeItWith(Disposable);

            _mapCacheDirectory = Cache.CacheFolder;
        }

        public long CalculateMapCacheSize()
        {
            return DirSize(new DirectoryInfo(_mapCacheDirectory));
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

        public IRxEditableValue<AccessMode> CurrentMapAccessMode => _currentMapAccessMode;

        public IEnumerable<AccessMode> AvailableAccessModes => Enum.GetValues<AccessMode>();

        public IRxEditableValue<GMapProvider> CurrentMapProvider => _currentMapProvider;

        public IEnumerable<GMapProvider> AvailableProviders => GMapProviders.List;
    }
}