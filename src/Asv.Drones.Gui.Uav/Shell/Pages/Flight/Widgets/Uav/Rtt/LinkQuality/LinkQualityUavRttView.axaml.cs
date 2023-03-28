using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Drones.Gui.Uav;

[ExportView(typeof(LinkQualityUavRttViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class LinkQualityUavRttView : UserControl
{
    public LinkQualityUavRttView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}