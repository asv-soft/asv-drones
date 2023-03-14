using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Drones.Gui.Uav;

[ExportView(typeof(HomeDistanceUavRttViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class HomeDistanceUavRttView : UserControl
{
    public HomeDistanceUavRttView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}