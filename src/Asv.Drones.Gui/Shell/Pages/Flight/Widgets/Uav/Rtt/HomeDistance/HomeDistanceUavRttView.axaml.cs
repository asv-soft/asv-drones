using Asv.Drones.Gui.Api;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Drones.Gui;

[ExportView(typeof(HomeDistanceUavRttViewModel))]
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