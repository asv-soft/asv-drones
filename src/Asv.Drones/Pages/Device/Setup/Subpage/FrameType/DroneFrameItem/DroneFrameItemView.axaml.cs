using Asv.Avalonia;
using Avalonia.Controls;

namespace Asv.Drones;

[ExportViewFor(typeof(DroneFrameItemViewModel))]
public partial class DroneFrameItemView : UserControl
{
    public DroneFrameItemView()
    {
        InitializeComponent();
    }
}
