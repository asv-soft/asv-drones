using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Drones.Gui.Sdr;

[ExportView(typeof(LinkQualitySdrRttViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class LinkQualitySdrRttView : UserControl
{
    public LinkQualitySdrRttView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}