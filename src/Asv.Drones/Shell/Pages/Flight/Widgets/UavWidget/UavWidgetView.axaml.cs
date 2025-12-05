using Asv.Avalonia;
using Avalonia.Controls;

namespace Asv.Drones;

[ExportViewFor(typeof(UavWidgetViewModel))]
public partial class UavWidgetView : UserControl
{
    public UavWidgetView()
    {
        InitializeComponent();
    }
}
