using System.Composition;
using Asv.Drones.Gui.Api;
using Avalonia;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui;

[ExportView(typeof(ShellViewModel))]
[Shared]
public partial class ShellView : ReactiveUserControl<ShellViewModel>
{
    public ShellView()
    {
        InitializeComponent();
    }

    private void OnAttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e)
    {
        var vm = this.DataContext as ShellViewModel;
    }
}