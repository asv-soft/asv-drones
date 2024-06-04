using Asv.Drones.Gui.Api;
using Avalonia.Controls;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui;

[ExportView(typeof(SavingBrowserViewModel))]
public partial class SavingBrowserView : ReactiveUserControl<SavingBrowserViewModel>
{
    public SavingBrowserView()
    {
        InitializeComponent();
    }
}