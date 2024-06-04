using Asv.Drones.Gui.Api;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Drones.Gui;

[ExportView(typeof(BatteryUavRttViewModel))]
public partial class BatteryUavRttView : UserControl
{
    public BatteryUavRttView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}