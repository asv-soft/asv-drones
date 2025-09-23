using Asv.Avalonia;
using Avalonia.Controls;

namespace Asv.Drones;

[ExportViewFor(typeof(SetupFrameTypeViewModel))]
public partial class SetupFrameTypeView : UserControl
{
    public SetupFrameTypeView()
    {
        InitializeComponent();
    }
}
