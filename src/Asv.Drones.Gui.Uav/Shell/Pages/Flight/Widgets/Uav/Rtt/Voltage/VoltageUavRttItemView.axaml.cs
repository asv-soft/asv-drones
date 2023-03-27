using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Drones.Gui.Uav;

[ExportView(typeof(VoltageUavRttItemViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class VoltageUavRttItemView : UserControl
{
    public VoltageUavRttItemView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}