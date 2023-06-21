using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Drones.Gui.Gbs;

[ExportView(typeof(BaseStationModeGbsRttViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class BaseStationModeGbsRttView : UserControl
{
    public BaseStationModeGbsRttView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}