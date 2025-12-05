using Asv.Avalonia;
using Avalonia.Controls;

namespace Asv.Drones;

[ExportViewFor(typeof(MotorItemViewModel))]
public partial class MotorItemView : UserControl
{
    public MotorItemView()
    {
        InitializeComponent();
    }
}
