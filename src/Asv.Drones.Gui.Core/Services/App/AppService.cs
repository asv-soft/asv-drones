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
        private readonly IEnumerable<Lazy<IAppStoreCopyProvider>> _storeCopyProviders;
        private readonly RxValue<IAppStore> _store;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly RxValue<string> _storeName;
        private const string _defaultStoreFileExtension = "asv";

        [ImportingConstructor]
        public AppService(IConfiguration cfgService, IAppPathInfo defaultPaths, IAppInfo info,
            [ImportMany]IEnumerable<Lazy<IAppStoreCopyProvider>> storeCopyProviders):base(cfgService)
        {
            _storeCopyProviders = storeCopyProviders;
            Paths = defaultPaths;
            Info = info;

            #region Store loading
            
            _store = new RxValue<IAppStore>().DisposeItWith(Disposable);
            
            if (Directory.Exists(Paths.AppStoreFolder) == false)
            {
                Directory.CreateDirectory(Paths.AppStoreFolder);
            }
            
            var lastPath = InternalGetConfig(_ => _.LastAppStorePath);
            if (lastPath.IsNullOrWhiteSpace() == false && File.Exists(lastPath))
            {
                OpenStore(lastPath);
            }
            else
            {
                CreateStoreDefault();
            }
            
            
            
            #endregion

            

        }



        public IAppInfo Info { get; }
        public IAppPathInfo Paths { get; }
        public IRxValue<IAppStore> Store => _store;

        public FilePickerFileType StoreFileFilter { get; } = new("Asv.Drones files")
        {
            Patterns = new [] { $"*.{_defaultStoreFileExtension}" },
            MimeTypes =new []{"text/*"},
            AppleUniformTypeIdentifiers = null
        };

        private void CreateStoreDefault()
        {
            string file = null;
            for (var i = 0; i < 1000; i++)
            {
                file = Path.Combine(Paths.AppStoreFolder, $"asv-drones-store-{i}.asv");
                if (File.Exists(file) == false) break;
            }
            if (file == null) 
                throw new Exception("Can't create new store file name");
            CreateStore(file, false);
        }

        public string GetSuggestedFileNameForStore()
        {
            var now = DateTime.Now;
            return $"asv-{now:yyyy-M-d-hh-mm-ss}.{_defaultStoreFileExtension}";
        }

        public string DefaultStoreFileExtension => _defaultStoreFileExtension;
        

        public void CreateStore(string filePath, bool copyFromCurrent)
        {
            _logger.Info($"Create store {filePath}. Copy from current: {copyFromCurrent}");
            var newStore = InternalCreateDb(filePath);
            var oldStore = Store.Value;
            if (oldStore != null)
            {
                _logger.Trace($"Copy data from old store to new store: {oldStore.SourceName} -> {newStore.SourceName}");
                foreach (var copyProvider in _storeCopyProviders)
                {
                    copyProvider.Value.TryCopy(oldStore, newStore);
                }
            }
            InternalSaveConfig(_=>_.LastAppStorePath = filePath);
            
            try
            {
                oldStore?.Dispose();
            }
            catch (Exception e)
            {
                _logger.Error($"Error to dispose old store {oldStore.SourceName} :{e.Message}");
                throw;
            }
            
            try
            {
                _store.OnNext(newStore);
            }
            catch (Exception e)
            {
                _logger.Error(e,$"Error to publish new store {newStore.SourceName}:{e.Message}");
                throw;
            }
    
        }

        public void OpenStore(string filePath)
        {
            try
            {
                _store.Value?.Dispose();
            }
            catch (Exception e)
            {
                _logger.Error($"Error to dispose store {_store.Value.SourceName} :{e.Message}");
                throw;
            }
            InternalSaveConfig(_=>_.LastAppStorePath = filePath);
            _logger.Info($"Open store {filePath}");
            _store.OnNext(InternalCreateDb(filePath));
        }
        
        private IAppStore InternalCreateDb(string filePath)
        {
            return new LiteDbAppStore(new LiteDatabase(new ConnectionString(filePath)
            {
                Connection = ConnectionType.Direct,
            }), filePath);
        }
    }
}