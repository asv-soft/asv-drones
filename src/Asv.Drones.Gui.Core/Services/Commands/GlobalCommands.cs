using System.ComponentModel.Composition;
using System.Reactive;
using System.Windows.Input;
using Avalonia.Platform.Storage;
using ReactiveUI;

namespace Asv.Drones.Gui.Core
{
    
    public interface IGlobalCommandsService
    {
        ReactiveCommand<Unit,Unit> OpenStore { get; }
        ReactiveCommand<Unit,Unit> CreateStore { get; }
    }
    
    [Export(typeof(IGlobalCommandsService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class GlobalCommandsService : IGlobalCommandsService
    {
        private readonly IAppService _app;
        private readonly ILogService _log;
        private readonly INavigationService _nav;

        [ImportingConstructor]
        public GlobalCommandsService(IAppService app, ILogService log, INavigationService nav)
        {
            _app = app ?? throw new ArgumentNullException(nameof(app));
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _nav = nav ?? throw new ArgumentNullException(nameof(nav));
            OpenStore = ReactiveCommand.CreateFromTask(InternalOpenStore);
            CreateStore = ReactiveCommand.CreateFromTask(InternalCreateStore);
        }

        private async Task InternalCreateStore(CancellationToken arg)
        {
            try
            {
                var currentFolder = TryGetCurrentFolder();
                //TODO: Localize
                var newFilePath = await _nav.ShowSaveFileDialogAsync("Create new store", 
                    currentFolder, 
                    _app.GetSuggestedFileNameForStore(), 
                    _app.DefaultStoreFileExtension, 
                    _app.StoreFileFilter);
                if (newFilePath == null) return;
                _app.CreateStore(newFilePath, true);
            }
            catch (Exception e)
            {
                //TODO: Localize
                _log.Error("Create file", e.Message, e);
            }
        }

        public ReactiveCommand<Unit,Unit> OpenStore { get; }
        public ReactiveCommand<Unit, Unit> CreateStore { get; }

        private async Task InternalOpenStore(CancellationToken cancel)
        {
            try
            {
                var currentFolder = TryGetCurrentFolder();
                var newFilePath = await _nav.ShowOpenFileDialogAsync(RS.GlobalCommands_OpenStoreDialogTitle, currentFolder, _app.StoreFileFilter);
                if (newFilePath == null) return;
                _app.OpenStore(newFilePath);
            }
            catch (Exception e)
            {
                //TODO: Localize
                _log.Error("Open file", e.Message, e);
            }
        }

        private string? TryGetCurrentFolder()
        {
            string? currentFolder = null;
            try
            {
                // try to get current folder from store source name
                currentFolder = Path.GetDirectoryName(_app.Store.Value.SourceName);
            }
            catch (Exception e)
            {
                // ignore
            }

            return currentFolder;
        }
    }
}