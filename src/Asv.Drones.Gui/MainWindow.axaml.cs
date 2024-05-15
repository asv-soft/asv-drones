using System;
using System.Composition;
using System.Composition.Hosting;
using Asv.Cfg;
using Avalonia;
using Avalonia.Controls;
using FluentAvalonia.UI.Windowing;

namespace Asv.Drones.Gui;

public partial class MainWindow : AppWindow
{
    private readonly IConfiguration _configuration = null!;
    private readonly CompositionHost _container = null!;

    public MainWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
        MinWidth = 800;
        MinHeight = 600;
        TitleBar.ExtendsContentIntoTitleBar = true;
        TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;
    }

    public MainWindow(CompositionHost container) : this()
    {
        _container = container;
        _configuration = container.GetExport<IConfiguration>();
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

        _container.Dispose();
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

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
}

public class MainWindowConfig
{
    public double Width { get; set; }
    public double Height { get; set; }
    public int PositionX { get; set; }
    public int PositionY { get; set; }
    public bool IsMaximized { get; set; }
}