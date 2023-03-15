using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Drones.Gui.Uav;

[ExportView(typeof(GpsUavRttViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class GpsUavRttView : UserControl
{
    public GpsUavRttView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}