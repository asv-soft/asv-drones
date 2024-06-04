using Asv.Drones.Gui.Api;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui;

[ExportView(typeof(SelectModeViewModel))]
public partial class SelectModeView : ReactiveUserControl<SelectModeViewModel>
{
    public SelectModeView()
    {
        InitializeComponent();
    }
}