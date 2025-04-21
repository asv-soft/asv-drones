using Asv.Avalonia;
using Avalonia.Controls;

namespace Asv.Drones;

[ExportViewFor(typeof(SetAltitudeDialogViewModel))]
public partial class SetAltitudeDialog : UserControl
{
    public SetAltitudeDialog()
    {
        InitializeComponent();
    }
}
