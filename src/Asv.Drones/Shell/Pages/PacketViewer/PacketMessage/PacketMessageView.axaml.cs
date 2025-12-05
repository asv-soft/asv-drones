using Asv.Avalonia;
using Avalonia.Controls;

namespace Asv.Drones;

[ExportViewFor<PacketMessageViewModel>]
public partial class PacketMessageView : UserControl
{
    public PacketMessageView()
    {
        InitializeComponent();
    }
}
