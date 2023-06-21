using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Drones.Gui.Uav;

[ExportView(typeof(BatteryUavRttViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
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