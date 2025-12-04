using Asv.Avalonia;
using Avalonia.Controls;

namespace Asv.Drones;

[ExportViewFor(typeof(SavePacketMessagesDialogViewModel))]
public partial class SavePacketMessagesDialogView : UserControl
{
    public SavePacketMessagesDialogView()
    {
        InitializeComponent();
    }
}
