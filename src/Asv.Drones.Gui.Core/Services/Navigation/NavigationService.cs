using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using Asv.Cfg;

namespace Asv.Drones.Gui.Core
{

    public class NavigationServiceConfig
    {

    }
    

    [Export(typeof(INavigationService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class NavigationService: ServiceWithConfigBase<NavigationServiceConfig>,INavigationService
    {
        private readonly CompositionContainer _container;
        private IShell _shell;
        public const string AsvUriTheme = "asv";

        [ImportingConstructor]
        public NavigationService(CompositionContainer container,IConfiguration cfgSvc):base(cfgSvc)
        {
            _container = container;
            
        }

        public void Init(IShell shellPage)
        {
            _shell = shellPage;
        }

        public void GoTo(Uri link)
        {
            if (link == null) throw new ArgumentNullException(nameof(link));
            if (link.Scheme.Equals(AsvUriTheme) == false)
            {
                throw new Exception($"Unknown uri scheme. Want {AsvUriTheme}. Got:{link.Scheme}");
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