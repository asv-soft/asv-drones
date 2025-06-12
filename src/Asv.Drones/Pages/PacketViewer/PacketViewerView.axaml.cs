using Asv.Avalonia;
using Avalonia.Controls;

namespace Asv.Drones;

[ExportViewFor(typeof(PacketViewerViewModel))]
public partial class PacketViewerView : UserControl
{
    public PacketViewerView()
    {
        InitializeComponent();
    }
}
