using Asv.Avalonia;
using Avalonia.Controls;

namespace Asv.Drones;

[ExportViewFor(typeof(RenameDialogViewModel))]
public partial class RenameDialogView : UserControl
{
    public RenameDialogView()
    {
        InitializeComponent();
    }
}
