using System.Composition;
using Asv.Avalonia;
using Avalonia;
using Avalonia.Controls;

namespace Asv.Drones;

public sealed class FlightPageViewConfig
{
    public double LeftColumnActualWidth { get; set; } = -1;
    public double CenterColumnActualWidth { get; set; } = -1;
    public double RightColumnActualWidth { get; set; } = -1;
    public double CenterRowActualHeight { get; set; } = -1;
    public double BottomRowActualHeight { get; set; } = -1;
}

[ExportViewFor(typeof(FlightPageViewModel))]
public partial class FlightPageView : UserControl
{
    private readonly ILayoutService _layoutService;
    private FlightPageViewConfig? _config;
    private WorkspacePanel? _workspace;

    public FlightPageView()
        : this(NullLayoutService.Instance)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [ImportingConstructor]
    public FlightPageView(ILayoutService layoutService)
    {
        _layoutService = layoutService;
        InitializeComponent();
    }

    private void WorkspacePanel_OnAttachedToVisualTree(
        object? sender,
        VisualTreeAttachmentEventArgs e
    )
    {
        if (Design.IsDesignMode)
        {
            return;
        }

        _workspace = sender as WorkspacePanel;
        LoadLayout();
    }

    private void OnWorkspaceChanged(object? sender, WorkspaceEventArgs workspaceEventArgs)
    {
        if (Design.IsDesignMode)
        {
            return;
        }

        SaveLayout(workspaceEventArgs);
    }

    private void LoadLayout()
    {
        _config = _layoutService.Get<FlightPageViewConfig>(this);

        if (_workspace is null)
        {
            return;
        }

        if (_config.LeftColumnActualWidth >= 0)
        {
            _workspace.LeftWidth = new GridLength(_config.LeftColumnActualWidth);
        }

        if (_config.CenterColumnActualWidth >= 0)
        {
            _workspace.CentralWidth = new GridLength(_config.CenterColumnActualWidth);
        }

        if (_config.RightColumnActualWidth >= 0)
        {
            _workspace.RightWidth = new GridLength(_config.RightColumnActualWidth);
        }

        if (_config.CenterRowActualHeight >= 0)
        {
            _workspace.CentralHeight = new GridLength(_config.CenterRowActualHeight);
        }

        if (_config.BottomRowActualHeight >= 0)
        {
            _workspace.BottomHeight = new GridLength(_config.BottomRowActualHeight);
        }
    }

    private void SaveLayout(WorkspaceEventArgs workspaceEventArgs)
    {
        if (_config is null)
        {
            return;
        }

        if (DataContext is null)
        {
            return;
        }

        _config.LeftColumnActualWidth = workspaceEventArgs.LeftColumnActualWidth;
        _config.CenterColumnActualWidth = workspaceEventArgs.CenterColumnActualWidth;
        _config.RightColumnActualWidth = workspaceEventArgs.RightColumnActualWidth;
        _config.CenterRowActualHeight = workspaceEventArgs.CenterRowActualHeight;
        _config.BottomRowActualHeight = workspaceEventArgs.BottomRowActualHeight;
        _layoutService.SetInMemory(this, _config);
    }
}
