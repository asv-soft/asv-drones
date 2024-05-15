using Asv.Drones.Gui.Api;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui;

[ExportView(typeof(SelectDownloadVehicleViewModel))]
public partial class SelectDownloadVehicleView : ReactiveUserControl<SelectDownloadVehicleViewModel>
{
    public SelectDownloadVehicleView()
    {
        InitializeComponent();
    }
}