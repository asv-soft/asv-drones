using Asv.Drones.Gui.Api;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui;

[ExportView(typeof(UploadFileDialogViewModel))]
public partial class UploadFileDialogView : ReactiveUserControl<UploadFileDialogViewModel>
{
    public UploadFileDialogView()
    {
        InitializeComponent();
    }
}
