using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Drones.Gui.Gbs;

[ExportView(typeof(LinkQualityGbsRttViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class LinkQualityGbsRttView : UserControl
{
    public LinkQualityGbsRttView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}