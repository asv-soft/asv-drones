using System.ComponentModel.Composition;
using System.Windows.Input;
using Asv.Avalonia.Map;
using Asv.Common;
using Material.Icons;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Core
{
    [Export(typeof(ISettingsPart))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class MapSettingsViewModel : SettingsPartBase
    {
        private static readonly Uri Uri = new(SettingsPartBase.Uri, "map");

        private readonly IMapService _mapService;
        private readonly INavigationService _navigationService;
        private readonly ILocalizationService _localizationService;

        public MapSettingsViewModel() : base(Uri)
        {
        }

        [ImportingConstructor]
        public MapSettingsViewModel(IMapService mapService, INavigationService navigationService, ILocalizationService localization) : this()
        {
            _mapService = mapService;
            _navigationService = navigationService;
            _localizationService = localization;

            _mapService.CurrentMapProvider.Subscribe(_ => CurrentMapProvider = _).DisposeItWith(Disposable);
            this.WhenAnyValue(_ => _.CurrentMapProvider).Subscribe(_mapService.CurrentMapProvider).DisposeItWith(Disposable);

            OpenFolderCommand = ReactiveCommand.CreateFromTask(OpenFolder).DisposeItWith(Disposable);
            UpdateDescription();
        }

        private async Task OpenFolder()
        {
            var path = await _navigationService.ShowOpenFolderDialogAsync(RS.MapSettingsViewModel_MapDialogTitle, _mapService.MapCacheDirectory);

            if (!string.IsNullOrEmpty(path))
            {
                _mapService.SetMapCacheDirectory(path);
                UpdateDescription();
            }
        }

        private void UpdateDescription()
        {
            MapStorageDescription = string.Format(RS.MapSettingsView_MapsInfo_Description, _mapService.MapCacheDirectory, _localizationService.ByteSize.ConvertToStringWithUnits(_mapService.CalculateMapCacheSize()));
        }

        public override int Order => 1;

        [Reactive]
        public GMapProvider CurrentMapProvider { get; set; }
        public IEnumerable<GMapProvider> AvailableProviders => _mapService.AvailableProviders;

        [Reactive]
        public string MapStorageDescription { get; set; }

        public ICommand OpenFolderCommand { get; }

        public string MapIcon => MaterialIconDataProvider.GetData(MaterialIconKind.Map);
        public string FolderIcon => MaterialIconDataProvider.GetData(MaterialIconKind.FolderOpen);
    }
}
