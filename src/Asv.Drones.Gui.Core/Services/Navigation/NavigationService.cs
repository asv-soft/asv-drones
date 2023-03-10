using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using Asv.Cfg;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Platform.Storage.FileIO;

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
        private IShell? _shell;
        private IStorageProvider? _windowStorageProvider;

        [ImportingConstructor]
        public NavigationService(CompositionContainer container,IConfiguration cfgSvc):base(cfgSvc)
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
            if (link.Scheme.Equals(WellKnownUri.UriScheme) == false)
            {
                throw new Exception($"Unknown uri scheme. Want {WellKnownUri.UriScheme}. Got:{link.Scheme}");
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

        public async Task<string?> ShowOpenFileDialogAsync(string title, string? suggestedStartLocation, params FilePickerFileType[] fileTypes)
        {
            if (_windowStorageProvider == null)
            {
                throw new Exception(
                    $"The order of loading services was broken. At this point the variable {nameof(_windowStorageProvider)} must be initialized.");
            }
            var file = await _windowStorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = title,
                SuggestedStartLocation = suggestedStartLocation != null ? new BclStorageFolder(suggestedStartLocation) : null,
                AllowMultiple = false,
                FileTypeFilter = fileTypes.Length == 0 ? null : fileTypes,
            });
            var selectedItem = file.FirstOrDefault();
            if (selectedItem == null) return null;
            return selectedItem.TryGetUri(out var uri) == false ? null : uri.AbsolutePath;
        }



        public async Task<string?> ShowOpenFolderDialogAsync(string title, string? suggestedStartLocation)
        {
            if (_windowStorageProvider == null)
            {
                throw new Exception(
                    $"The order of loading services was broken. At this point the variable {nameof(_windowStorageProvider)} must be initialized.");
            }
            if (title == null) throw new ArgumentNullException(nameof(title));
            var file = await _windowStorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                SuggestedStartLocation = suggestedStartLocation != null ? new BclStorageFolder(suggestedStartLocation):null,
                AllowMultiple = false,
                Title = title
            });
            var selectedItem = file.FirstOrDefault();
            if (selectedItem == null) return null;
            return selectedItem.TryGetUri(out var uri) == false ? null : uri.AbsolutePath;
        }

        public async Task<string?> ShowSaveFileDialogAsync(string title, string? suggestedStartLocation, string? suggestedFileName, string? defaultExtension, params FilePickerFileType[] fileTypes)
        {
            if (_windowStorageProvider == null)
            {
                throw new Exception(
                    $"The order of loading services was broken. At this point the variable {nameof(_windowStorageProvider)} must be initialized.");
            }
            var file = await _windowStorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = title,
                SuggestedStartLocation = suggestedStartLocation != null ? new BclStorageFolder(suggestedStartLocation) : null,
                SuggestedFileName = suggestedFileName,
                DefaultExtension = defaultExtension,
                FileTypeChoices = fileTypes.Length == 0 ? null : fileTypes,
                ShowOverwritePrompt = true,
            });
            if (file == null) return null;
            return file.TryGetUri(out var uri) == false ? null : uri.AbsolutePath;
        }
    }
    
}