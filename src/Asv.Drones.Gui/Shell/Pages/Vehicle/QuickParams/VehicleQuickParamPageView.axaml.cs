using Asv.Drones.Gui.Api;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui;

[ExportView(typeof(VehicleQuickParamPageViewModel))]
public partial class VehicleQuickParamPageView : ReactiveUserControl<VehicleQuickParamPageViewModel>
{
    public VehicleQuickParamPageView()
    {
        InitializeComponent();
    }
}