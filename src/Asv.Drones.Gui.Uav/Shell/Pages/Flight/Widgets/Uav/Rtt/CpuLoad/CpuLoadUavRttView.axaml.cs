using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Drones.Gui.Uav;


[ExportView(typeof(CpuLoadUavRttViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class CpuLoadUavRttView : UserControl
{
    public CpuLoadUavRttView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}