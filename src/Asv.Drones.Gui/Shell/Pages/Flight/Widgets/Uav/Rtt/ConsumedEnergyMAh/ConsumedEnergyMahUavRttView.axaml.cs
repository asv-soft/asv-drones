using Asv.Drones.Gui.Api;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Drones.Gui;

[ExportView(typeof(ConsumedEnergyMAhUavRttViewModel))]
public partial class ConsumedEnergyMahUavRttView : UserControl
{
    public ConsumedEnergyMahUavRttView()
    {
        InitializeComponent();
    }
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}