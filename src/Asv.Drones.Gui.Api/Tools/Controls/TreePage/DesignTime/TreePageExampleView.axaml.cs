using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Drones.Gui.Api;

[ExportView(typeof(TreePageExampleViewModel))]
public partial class TreePageExampleView : UserControl
{
    public TreePageExampleView()
    {
        InitializeComponent();
    }
}