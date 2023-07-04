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
        private readonly ILocalizationService _localizationService;

        public MapSettingsViewModel() : base(Uri)
        {
        }

        [ImportingConstructor]
        public MapSettingsViewModel(IMapService mapService, INavigationService navigationService, ILocalizationService localization) : this()
        {
            _mapService = mapService;
            _localizationService = localization;

            _mapService.CurrentMapProvider.Subscribe(_ => CurrentMapProvider = _).DisposeItWith(Disposable);
            this.WhenAnyValue(_ => _.CurrentMapProvider).Subscribe(_mapService.CurrentMapProvider).DisposeItWith(Disposable);

            ClearMapStorageCommand = ReactiveCommand.Create(ClearMapStorage).DisposeItWith(Disposable);
            UpdateDescription();
        }
        
        private void UpdateDescription()
        {
            MapStorageDescription = string.Format(RS.MapSettingsView_MapsInfo_Description, _mapService.MapCacheDirectory, 
                _localizationService.ByteSize.ConvertToStringWithUnits(_mapService.CalculateMapCacheSize()));
        }
        
        private void ClearMapStorage()
        {
            if (string.IsNullOrWhiteSpace(_mapService.MapCacheDirectory)) return;
            
            try
            {
                var dir = new DirectoryInfo(_mapService.MapCacheDirectory);
                
                foreach (var subDirectory in dir.GetDirectories())
                {
                    subDirectory.Delete(recursive: true);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
            UpdateDescription();
        }

        public override int Order => 1;

        [Reactive]
        public GMapProvider CurrentMapProvider { get; set; }
        public IEnumerable<GMapProvider> AvailableProviders => _mapService.AvailableProviders;

        [Reactive]
        public string MapStorageDescription { get; set; }
        
        public ICommand ClearMapStorageCommand { get; set; }

        public string MapIcon => MaterialIconDataProvider.GetData(MaterialIconKind.Map);
        public string FolderIcon => MaterialIconDataProvider.GetData(MaterialIconKind.FolderOpen);
    }
}
