using Asv.Avalonia;
using Avalonia.Controls;

namespace Asv.Drones;

[ExportViewFor(typeof(TryCloseWithApprovalDialogViewModel))]
public partial class TryCloseWithApprovalDialogView : UserControl
{
    public TryCloseWithApprovalDialogView()
    {
        InitializeComponent();
    }
}
