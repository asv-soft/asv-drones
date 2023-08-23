using System;
using System.Runtime.InteropServices;
using Asv.Cfg;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;
using FluentAvalonia.Styling;
using FluentAvalonia.UI.Media;
using FluentAvalonia.UI.Windowing;

namespace Asv.Drones.Gui.Views;

public partial class MainWindow : AppWindow
{
    private readonly IConfiguration _configuration = null!;

    public MainWindow()
    {
        InitializeComponent();
#if DEBUG || DEBUGPLUGINS || DEBUGLIBS
        this.AttachDevTools();
#endif
        MinWidth = 450;
        MinHeight = 400;
        TitleBar.ExtendsContentIntoTitleBar = true;
        TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;
        
        Application.Current.ActualThemeVariantChanged += ApplicationActualThemeVariantChanged;
    }
    
    public MainWindow(IConfiguration configuration) : this()
    {
        _configuration = configuration;
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        
        if (_configuration == null) return;
        
        MainWindowConfig shellViewConfig;
        if (WindowState == WindowState.Maximized)
        {
            shellViewConfig = new MainWindowConfig
            {
                IsMaximized = true
            };
        }
        else
        {
            shellViewConfig = new MainWindowConfig
            {
                Height = Height,
                Width = Width,
                PositionX = Position.X,
                PositionY = Position.Y
            };
        }

        _configuration.Set(shellViewConfig);
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        var thm = ActualThemeVariant;
        // Enable Mica on Windows 11
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // TODO: add Windows version to CoreWindow
            if (IsWindows11 && thm != FluentAvaloniaTheme.HighContrastTheme)
            {
                //TransparencyBackgroundFallback = Brushes.Transparent;
                //TransparencyLevelHint = WindowTransparencyLevel.Mica;

                //TryEnableMicaEffect();
            }
        }

        if (_configuration == null) return;

        var shellViewConfig = _configuration.Get<MainWindowConfig>();

        if (shellViewConfig.IsMaximized)
        {
            WindowState = WindowState.Maximized;
            return;
        }

        var totalWidth = 0;
        var totalHeight = 0;

        foreach (var scr in Screens.All)
        {
            totalWidth += scr.Bounds.Width;
            totalHeight += scr.Bounds.Height;
        }

        if (shellViewConfig.PositionX > totalWidth || shellViewConfig.PositionY > totalHeight)
        {
            Position = new PixelPoint(0, 0);
        }
        else
        {
            Position = new PixelPoint(shellViewConfig.PositionX, shellViewConfig.PositionY);
        }

        if (shellViewConfig.Height > totalHeight || shellViewConfig.Width > totalWidth)
        {
            var scrBounds = Screens.Primary.Bounds;

            Height = scrBounds.Height * 0.9;
            Width = scrBounds.Width * 0.9;

            Position = new PixelPoint(0, 0);
        }
        else
        {
            Height = shellViewConfig.Height;
            Width = shellViewConfig.Width;
        }
    }

    private void ApplicationActualThemeVariantChanged(object sender, EventArgs e)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // TODO: add Windows version to CoreWindow
            if (IsWindows11 && ActualThemeVariant != FluentAvaloniaTheme.HighContrastTheme)
            {
                TryEnableMicaEffect();
            }
            else if (ActualThemeVariant != FluentAvaloniaTheme.HighContrastTheme)
            {
                // Clear the local value here, and let the normal styles take over for HighContrast theme
                SetValue(BackgroundProperty, AvaloniaProperty.UnsetValue);
            }
        }
    }
    private void TryEnableMicaEffect()
    {
        // The background colors for the Mica brush are still based around SolidBackgroundFillColorBase resource
        // BUT since we can't control the actual Mica brush color, we have to use the window background to create
        // the same effect. However, we can't use SolidBackgroundFillColorBase directly since its opaque, and if
        // we set the opacity the color become lighter than we want. So we take the normal color, darken it and 
        // apply the opacity until we get the roughly the correct color
        // NOTE that the effect still doesn't look right, but it suffices. Ideally we need access to the Mica
        // CompositionBrush to properly change the color but I don't know if we can do that or not
        if (ActualThemeVariant == ThemeVariant.Dark)
        {
            var color = this.TryFindResource("SolidBackgroundFillColorBase",
                ThemeVariant.Dark, out var value) ? (Color2)(Color)value : new Color2(32, 32, 32);

            color = color.LightenPercent(-0.8f);

            Background = new ImmutableSolidColorBrush(color, 0.78);
        }
        else if (ActualThemeVariant == ThemeVariant.Light)
        {
            // Similar effect here
            var color = this.TryFindResource("SolidBackgroundFillColorBase",
                ThemeVariant.Light, out var value) ? (Color2)(Color)value : new Color2(243, 243, 243);

            color = color.LightenPercent(0.5f);

            Background = new ImmutableSolidColorBrush(color, 0.9);
        }
    }
}

public class MainWindowConfig
{
    public double Width { get; set; }
    public double Height { get; set; }
    public int PositionX { get; set; }
    public int PositionY { get; set; }
    public bool IsMaximized { get; set; }
}