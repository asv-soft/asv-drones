using Asv.Avalonia;
using Avalonia.Controls;

namespace Asv.Drones;

public partial class FlightModePageView : UserControl
{
    private readonly IDisposable? _layout;

    public FlightModePageView()
    {
        InitializeComponent();

        _layout = this.RegisterWorkspaceLayout(
            $"{nameof(FlightModePageView)}.workspace",
            PART_Workspace
        );
        DetachedFromVisualTree += (_, _) => _layout.Dispose();
    }
}
