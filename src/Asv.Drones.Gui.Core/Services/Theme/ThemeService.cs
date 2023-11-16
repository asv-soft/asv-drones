using System.ComponentModel.Composition;
using Asv.Cfg;
using Asv.Common;
using Avalonia;
using Avalonia.Styling;
using FluentAvalonia.Styling;

namespace Asv.Drones.Gui.Core
{
    public class ThemeServiceConfig
    {
        public string? Theme { get; set; }
    }

    [Export(typeof(IThemeService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ThemeService : ServiceWithConfigBase<ThemeServiceConfig>, IThemeService
    {
        private readonly ThemeItem[] _themes = {
            new(FluentAvaloniaTheme.DarkModeString, RS.ThemeService_Themes_Dark, ThemeVariant.Dark),
            new(FluentAvaloniaTheme.LightModeString, RS.ThemeService_Themes_Light, ThemeVariant.Light),
        };

        [ImportingConstructor]
        public ThemeService(IConfiguration cfgSvc):base(cfgSvc)
        {
            Application.Current.RequestedThemeVariant = ThemeVariant.Default;
            var selectedTheme = default(ThemeItem);

            var themeFromConfig = InternalGetConfig(_ => _.Theme);
            if (string.IsNullOrWhiteSpace(themeFromConfig) == false)
            {
                selectedTheme = _themes.FirstOrDefault(_ => _.Id.Equals(themeFromConfig));
            }

            selectedTheme ??= _themes.First();
            CurrentTheme = new RxValue<ThemeItem>(selectedTheme).DisposeItWith(Disposable);
            CurrentTheme.Subscribe(SetTheme).DisposeItWith(Disposable);
        }

        public IEnumerable<ThemeItem> Themes =>_themes;
        public IRxEditableValue<ThemeItem> CurrentTheme { get; }

        private void SetTheme(ThemeItem theme)
        {
            if (theme == null) throw new ArgumentNullException(nameof(theme));
            Application.Current.RequestedThemeVariant = theme.Theme;
            
            InternalSaveConfig(_ => _.Theme = theme.Id);
        }
    }
}