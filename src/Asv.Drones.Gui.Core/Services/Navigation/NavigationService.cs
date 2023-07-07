#nullable enable
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using Asv.Cfg;
using Avalonia.Platform.Storage;
using Avalonia.Platform.Storage.FileIO;

namespace Asv.Drones.Gui.Core
{

    public class NavigationServiceConfig
    {

    }
    

    [Export(typeof(INavigationService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class NavigationService: ServiceWithConfigBase<NavigationServiceConfig>, INavigationService
    {
        public const string UriScheme = "asv";
        private readonly CompositionContainer _container;
        private IShell? _shell;
        private IStorageProvider? _windowStorageProvider;

        [ImportingConstructor]
        public NavigationService(CompositionContainer container, IConfiguration cfgSvc):base(cfgSvc)
        {
            _container = container;
        }

        public void InitStorageProvider(IStorageProvider windowStorageProvider)
        {
            _windowStorageProvider = windowStorageProvider ?? throw new ArgumentNullException(nameof(windowStorageProvider));
        }

        public void Init(IShell shellPage)
        {
            _shell = shellPage ?? throw new ArgumentNullException(nameof(shellPage));
        }

        public void GoTo(Uri link)
        {
            if (_shell == null)
            {
                throw new Exception(
                    $"The order of loading services was broken. At this point the variable {nameof(_shell)} must be initialized.");
            }
            if (link == null) throw new ArgumentNullException(nameof(link));
            if (link.Scheme.Equals(UriScheme) == false)
            {
                throw new Exception($"Unknown uri scheme. Want {UriScheme}. Got:{link.Scheme}");
            }
            
            var current = _shell.CurrentPage;
            if (current?.GetType().GetCustomAttribute<PartCreationPolicyAttribute>()!.CreationPolicy == CreationPolicy.NonShared)
            {
                current.Dispose();
            }
            var viewModel = _container.GetExportedValue<IShellPage>(link.AbsolutePath);
            
            viewModel?.SetArgs(link);
            _shell.CurrentPage = viewModel;
        }

        
    }
    
}