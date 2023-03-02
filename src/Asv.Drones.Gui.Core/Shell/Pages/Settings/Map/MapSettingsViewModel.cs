using System.ComponentModel.Composition;
using System.Windows.Input;
using Asv.Avalonia.Map;
using Asv.Common;
using Avalonia.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Core
{
    [Export(typeof(ISettingsPart))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class MapSettingsViewModel : ViewModelBase, ISettingsPart
    {
        private readonly IMapService _mapService;
        private readonly INavigationService _navigationService;

        public MapSettingsViewModel() : base(new (WellKnownUri.ShellPageMapSettings))
        {
        }

        [ImportingConstructor]
        public MapSettingsViewModel(IMapService mapService, INavigationService navigationService, ILocalizationService localization) : this()
        {
            _mapService = mapService;
            _navigationService = navigationService;

            _mapService.CurrentMapProvider.Subscribe(_ => CurrentMapProvider = _).DisposeItWith(Disposable);
            this.WhenAnyValue(_ => _.CurrentMapProvider).Subscribe(_mapService.CurrentMapProvider).DisposeItWith(Disposable);

            OpenFolderCommand = ReactiveCommand.CreateFromTask(OpenFolder).DisposeItWith(Disposable);
            MapSize = localization.ByteSize.GetValueWithUnits(_mapService.CalculateMapCacheSize());
        }

        public async Task OpenFolder()
        {
            var path = await _navigationService.ShowOpenFolderDialogAsync("Maps", null);

            if (!string.IsNullOrEmpty(path))
            {
                _mapService.SetMapCacheDirectory(path);
            }
        }

        public int Order => 1;

        [Reactive]
        public GMapProvider CurrentMapProvider { get; set; }
        public IEnumerable<GMapProvider> AvailableProviders => _mapService.AvailableProviders;

        public string MapDirectory => _mapService.MapCacheDirectory;

        public string MapSize { get; set; }

        public ICommand OpenFolderCommand { get; }
    }
}
