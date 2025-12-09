using Asv.Avalonia;
using Avalonia.Controls;

namespace Asv.Drones;

[ExportViewFor(typeof(SetupMotorsViewModel))]
public partial class SetupMotorsView : UserControl
{
    public SetupMotorsView()
    {
        InitializeComponent();
    }
}
