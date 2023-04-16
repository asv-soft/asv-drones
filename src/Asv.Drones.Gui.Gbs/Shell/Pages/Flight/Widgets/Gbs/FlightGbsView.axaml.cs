using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Gbs;

[ExportView(typeof(FlightGbsViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class FlightGbsView : ReactiveUserControl<FlightGbsViewModel>
{
    public FlightGbsView()
    {
        InitializeComponent();
    }
}