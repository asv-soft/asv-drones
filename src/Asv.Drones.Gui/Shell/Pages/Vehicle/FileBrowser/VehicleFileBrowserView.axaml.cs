using Asv.Drones.Gui.Api;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui;

[ExportView(typeof(VehicleFileBrowserViewModel))]
public partial class VehicleFileBrowserView : ReactiveUserControl<VehicleFileBrowserViewModel>
{
    public VehicleFileBrowserView()
    {
        InitializeComponent();
    }
}
