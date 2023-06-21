using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Drones.Gui.Gbs;

[ExportView(typeof(DGpsRateGbsRttViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class DGpsRateGbsRttView : UserControl
{
    public DGpsRateGbsRttView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}