using Avalonia.Controls;

namespace Asv.Drones.Gui.Api;

[ExportView(typeof(TreeGroupViewModel))]
public partial class TreeGroupView : UserControl
{
    public TreeGroupView()
    {
        InitializeComponent();
    }
}