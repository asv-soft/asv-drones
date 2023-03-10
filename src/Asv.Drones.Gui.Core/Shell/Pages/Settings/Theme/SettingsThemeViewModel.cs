using System.ComponentModel.Composition;
using Asv.Common;
using Material.Icons;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Core
{

    [Export(typeof(ISettingsPart))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class SettingsThemeViewModel : SettingsPartBase
    {
        
        private static readonly Uri Uri = new(SettingsPartBase.Uri, "theme");
        
        private readonly IThemeService _themeService;
        private readonly ILocalizationService _localization;
        
        public override int Order => 0;
        
        public SettingsThemeViewModel() : base(Uri)
        {
            
        }

        [ImportingConstructor]
        public SettingsThemeViewModel(IThemeService themeService, ILocalizationService localization) : this()
        {
            _themeService = themeService;
            _localization = localization;

            _themeService.CurrentTheme.Subscribe(_ => SelectedTheme = _).DisposeItWith(Disposable);
            this.WhenAnyValue(_ => _.SelectedTheme)
                .Subscribe(_themeService.CurrentTheme)
                .DisposeItWith(Disposable);

            _themeService.FlowDirection.Subscribe(_ => FlowDirection = _)
                .DisposeItWith(Disposable);
            this.WhenAnyValue(_ => _.FlowDirection)
                .Subscribe(_themeService.FlowDirection)
                .DisposeItWith(Disposable);

            this.WhenAnyValue(_ => _.SelectedLanguage)
                .Subscribe(_ => IsRebootRequired = _ != _localization.CurrentLanguage.Value).DisposeItWith(Disposable);
            
            _localization.CurrentLanguage.Subscribe(_ => SelectedLanguage = _).DisposeItWith(Disposable);
            this.WhenAnyValue(_ => _.SelectedLanguage)
                .Subscribe(_localization.CurrentLanguage)
                .DisposeItWith(Disposable);
        }

        public IEnumerable<ThemeItem> AppThemes => _themeService.Themes;

        [Reactive]
        public ThemeItem SelectedTheme { get; set; }

        [Reactive]
        public FlowDirectionItem FlowDirection { get; set; }

        public IEnumerable<FlowDirectionItem> AppFlowDirections => _themeService.FlowDirections;

        public string LanguageIcon => MaterialIconDataProvider.GetData(MaterialIconKind.Translate);

        [Reactive]
        public LanguageInfo SelectedLanguage { get; set; }

        public IEnumerable<LanguageInfo> AppLanguages => _localization.AvailableLanguages;
    }
}