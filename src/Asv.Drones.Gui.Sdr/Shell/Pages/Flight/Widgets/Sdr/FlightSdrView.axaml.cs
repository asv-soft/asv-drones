using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Sdr;

[ExportView(typeof(FlightSdrViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class FlightSdrView : ReactiveUserControl<FlightSdrViewModel>
{
    public FlightSdrView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}