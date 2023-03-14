using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Drones.Gui.Uav;

[ExportView(typeof(FlightTimeUavRttViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class FlightTimeUavRttView : UserControl
{
    public FlightTimeUavRttView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}