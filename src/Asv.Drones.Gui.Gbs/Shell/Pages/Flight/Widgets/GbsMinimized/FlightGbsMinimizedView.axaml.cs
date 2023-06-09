using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Gbs;

[ExportView(typeof(FlightGbsMinimizedViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class FlightGbsMinimizedView : ReactiveUserControl<FlightGbsMinimizedViewModel>
{
    public FlightGbsMinimizedView()
    {
        InitializeComponent();
    }
}