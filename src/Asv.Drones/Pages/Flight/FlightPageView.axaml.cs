using Asv.Avalonia;
using Avalonia.Controls;

namespace Asv.Drones;

[ExportViewFor(typeof(FlightPageViewModel))]
public partial class FlightPageView : UserControl
{
    public FlightPageView()
    {
        InitializeComponent();
    }
}
