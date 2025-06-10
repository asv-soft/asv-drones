using Asv.Avalonia;
using Avalonia.Controls;

namespace Asv.Drones;

[ExportViewFor(typeof(BurstDownloadDialogViewModel))]
public partial class BurstDownloadDialogView : UserControl
{
    public BurstDownloadDialogView()
    {
        InitializeComponent();
    }
}
