using Asv.Drones.Gui.Api;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Drones.Gui;

[ExportView(typeof(GpsUavRttViewModel))]
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