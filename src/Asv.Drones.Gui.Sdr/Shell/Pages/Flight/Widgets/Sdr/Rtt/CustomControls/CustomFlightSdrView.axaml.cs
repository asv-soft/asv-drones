using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Sdr;

[ExportView(typeof(CustomFlightSdrViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class CustomFlightSdrView : ReactiveUserControl<CustomFlightSdrViewModel>
{
    public CustomFlightSdrView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}