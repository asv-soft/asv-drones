using Asv.Avalonia;
using Avalonia;
using Avalonia.Controls;
using R3;

namespace Asv.Drones;

internal sealed class FlightPageViewConfig
{
    public double LeftColumnActualWidth { get; set; } = -1;
    public double CenterColumnActualWidth { get; set; } = -1;
    public double RightColumnActualWidth { get; set; } = -1;
    public double CenterRowActualHeight { get; set; } = -1;
    public double BottomRowActualHeight { get; set; } = -1;
}

public partial class FlightModePageView : UserControl
{
    private WorkspacePanel _workspace;
    private IDisposable _layout;
    private SynchronizedReactiveProperty<WorkspaceEventArgs> _layoutChanged;

    public FlightModePageView()
    {
        InitializeComponent();
    }

    private void WorkspacePanel_OnDetachedFromVisualTree(
        object? sender,
        VisualTreeAttachmentEventArgs e
    )
    {
        if (Design.IsDesignMode)
        {
            return;
        }

        _layout.Dispose();
        _layoutChanged.Dispose();
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

        _workspace =
            sender as WorkspacePanel
            ?? throw new Exception($"Sender is not {nameof(WorkspacePanel)}");
        _layoutChanged = new SynchronizedReactiveProperty<WorkspaceEventArgs>();
        _layout = this.RegisterLayout(
            nameof(FlightModePageView),
            LoadLayout,
            SaveLayout,
            _layoutChanged.Skip(1).ObserveOnUIThreadDispatcher()
        );
    }

    private void OnWorkspaceChanged(object? sender, WorkspaceEventArgs workspaceEventArgs)
    {
        if (Design.IsDesignMode)
        {
            return;
        }

        _layoutChanged.OnNext(workspaceEventArgs);
    }

    private void LoadLayout(FlightPageViewConfig config)
    {
        if (config.LeftColumnActualWidth >= 0)
        {
            _workspace.LeftWidth = new GridLength(config.LeftColumnActualWidth);
        }

        if (config.CenterColumnActualWidth >= 0)
        {
            _workspace.CentralWidth = new GridLength(config.CenterColumnActualWidth);
        }

        if (config.RightColumnActualWidth >= 0)
        {
            _workspace.RightWidth = new GridLength(config.RightColumnActualWidth);
        }

        if (config.CenterRowActualHeight >= 0)
        {
            _workspace.CentralHeight = new GridLength(config.CenterRowActualHeight);
        }

        if (config.BottomRowActualHeight >= 0)
        {
            _workspace.BottomHeight = new GridLength(config.BottomRowActualHeight);
        }
    }

    private FlightPageViewConfig? SaveLayout()
    {
        var layout = _layoutChanged.CurrentValue;

        return new FlightPageViewConfig()
        {
            LeftColumnActualWidth = layout.LeftColumnActualWidth,
            CenterColumnActualWidth = layout.CenterColumnActualWidth,
            RightColumnActualWidth = layout.RightColumnActualWidth,
            CenterRowActualHeight = layout.CenterRowActualHeight,
            BottomRowActualHeight = layout.BottomRowActualHeight,
        };
    }
}
