using System.ComponentModel.Composition;
using Asv.Cfg;
using Asv.Common;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using FluentAvalonia.Styling;
using Newtonsoft.Json.Linq;

namespace Asv.Drones.Gui.Core
{
    public class ThemeServiceConfig
    {
        public string? Theme { get; set; }
        public FlowDirection? FlowDirection { get; set; }
    }

    

    [Export(typeof(IThemeService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ThemeService : ServiceWithConfigBase<ThemeServiceConfig>, IThemeService
    {
        private readonly ThemeItem[] _themes = {
            new(FluentAvaloniaTheme.DarkModeString, RS.ThemeService_Themes_Dark),
            new(FluentAvaloniaTheme.LightModeString, RS.ThemeService_Themes_Light),
            new(FluentAvaloniaTheme.HighContrastModeString, RS.ThemeService_Themes_Contrast),
        };

        private readonly FlowDirectionItem[] _flowDirections = {
            new(global::Avalonia.Media.FlowDirection.LeftToRight,RS.ThemeService_FlowDirections_LeftToRight),
            new(global::Avalonia.Media.FlowDirection.RightToLeft,RS.ThemeService_FlowDirections_RightToLeft),
        };


        private readonly FluentAvaloniaTheme _faTheme;
        private readonly object _sync = new();

        [ImportingConstructor]
        public ThemeService(IConfiguration cfgSvc):base(cfgSvc)
        {
            _faTheme = AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>() ?? throw new InvalidOperationException();
            var selectedTheme = default(ThemeItem);

            var themeFromConfig = InternalGetConfig(_ => _.Theme);
            if (string.IsNullOrWhiteSpace(themeFromConfig) == false)
            {
                selectedTheme = _themes.FirstOrDefault(_ => _.Id.Equals(themeFromConfig));
            }

            selectedTheme ??= _themes.First();
            CurrentTheme = new RxValue<ThemeItem>(selectedTheme).DisposeItWith(Disposable);
            CurrentTheme.Subscribe(SetTheme).DisposeItWith(Disposable);

            var flowFromConfig = InternalGetConfig(_ => _.FlowDirection);
            var flowDirection = default(FlowDirectionItem);

            if (flowFromConfig == null)
            {
                flowDirection = _flowDirections.FirstOrDefault(_ => _.Id == flowFromConfig);
            }

            flowDirection ??= _flowDirections.First();
            FlowDirection = new RxValue<FlowDirectionItem>(flowDirection).DisposeItWith(Disposable);
            FlowDirection.Subscribe(SetFlowDirection).DisposeItWith(Disposable);

        }

        public IEnumerable<ThemeItem> Themes =>_themes;
        public IRxEditableValue<ThemeItem> CurrentTheme { get; }
        public IRxEditableValue<FlowDirectionItem> FlowDirection { get; }
        public IEnumerable<FlowDirectionItem> FlowDirections => _flowDirections;

        private void SetTheme(ThemeItem theme)
        {
            if (theme == null) throw new ArgumentNullException(nameof(theme));
            _faTheme.RequestedTheme = theme.Id;
            InternalSaveConfig(_ => _.Theme = theme.Id);
        }

        private void SetFlowDirection(FlowDirectionItem item)
        {
            var mainWindow = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (mainWindow != null) mainWindow.FlowDirection = item.Id;
            InternalSaveConfig(_=>_.FlowDirection = item.Id);
        }
        
    }
}