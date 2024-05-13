using Asv.Drones.Gui.Api;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui;

[ExportView(typeof(SelectUploadVehicleViewModel))]
public partial class SelectUploadVehicleView : ReactiveUserControl<SelectUploadVehicleViewModel>
{
    public SelectUploadVehicleView()
    {
        InitializeComponent();
    }
}