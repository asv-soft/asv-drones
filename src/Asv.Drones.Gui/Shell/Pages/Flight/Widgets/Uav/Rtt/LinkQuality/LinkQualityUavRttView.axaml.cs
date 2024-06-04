using Asv.Drones.Gui.Api;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Drones.Gui;

[ExportView(typeof(LinkQualityUavRttViewModel))]
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