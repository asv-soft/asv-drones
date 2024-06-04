using Asv.Drones.Gui.Api;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Drones.Gui;

[ExportView(typeof(FlightTimeUavRttViewModel))]
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