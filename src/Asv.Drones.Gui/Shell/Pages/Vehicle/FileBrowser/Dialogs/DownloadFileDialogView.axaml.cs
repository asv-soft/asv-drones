using Asv.Drones.Gui.Api;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui;

[ExportView(typeof(DownloadFileDialogViewModel))]
public partial class DownloadFileDialogView : ReactiveUserControl<DownloadFileDialogViewModel>
{
    public DownloadFileDialogView()
    {
        InitializeComponent();
    }
}
