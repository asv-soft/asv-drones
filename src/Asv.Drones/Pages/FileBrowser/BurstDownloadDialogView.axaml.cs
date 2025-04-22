using Asv.Avalonia;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Drones;

[ExportViewFor(typeof(BurstDownloadDialogViewModel))]
public partial class BurstDownloadDialogView : UserControl
{
    public BurstDownloadDialogView()
    {
        InitializeComponent();
    }
}
