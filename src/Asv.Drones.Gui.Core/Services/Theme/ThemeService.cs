using System.ComponentModel.Composition;
using Asv.Cfg;
using Asv.Common;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Styling;
using FluentAvalonia.Styling;

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
            new(FluentAvaloniaTheme.DarkModeString, RS.ThemeService_Themes_Dark, ThemeVariant.Dark),
            new(FluentAvaloniaTheme.LightModeString, RS.ThemeService_Themes_Light, ThemeVariant.Light),
        };

        private readonly FlowDirectionItem[] _flowDirections = {
            new(global::Avalonia.Media.FlowDirection.LeftToRight,RS.ThemeService_FlowDirections_LeftToRight),
            new(global::Avalonia.Media.FlowDirection.RightToLeft,RS.ThemeService_FlowDirections_RightToLeft),
        };


        
        private readonly object _sync = new();

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
            Application.Current.RequestedThemeVariant = theme.Theme;
            
            InternalSaveConfig(_ => _.Theme = theme.Id);
        }

        private void SetFlowDirection(FlowDirectionItem value)
        {
            var lifetime = Application.Current.ApplicationLifetime;
            if (lifetime is IClassicDesktopStyleApplicationLifetime cdl)
            {
                if (cdl.MainWindow.FlowDirection == value.Id)
                    return;
                cdl.MainWindow.FlowDirection = value.Id;
            }
            else if (lifetime is ISingleViewApplicationLifetime single)
            {
                var mainWindow = TopLevel.GetTopLevel(single.MainView);
                if (mainWindow.FlowDirection == value.Id)
                    return;
                mainWindow.FlowDirection = value.Id;
            }
            
            
            InternalSaveConfig(_=>_.FlowDirection = value.Id);
        }
        
    }
}