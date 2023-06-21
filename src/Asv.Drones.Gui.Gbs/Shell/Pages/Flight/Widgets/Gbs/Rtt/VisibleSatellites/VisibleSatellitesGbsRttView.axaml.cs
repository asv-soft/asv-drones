using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Drones.Gui.Gbs;

[ExportView(typeof(VisibleSatellitesGbsRttViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class VisibleSatellitesGbsRttView : UserControl
{
    public VisibleSatellitesGbsRttView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}