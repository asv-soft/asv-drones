using Asv.Drones.Gui.Api;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui;

[ExportView(typeof(FlightUavViewModel))]
public partial class FlightUavView : ReactiveUserControl<FlightUavViewModel>
{
    public FlightUavView()
    {
        InitializeComponent();
    }
}