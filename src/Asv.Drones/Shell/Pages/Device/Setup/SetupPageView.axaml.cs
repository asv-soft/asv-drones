using Asv.Avalonia;
using Avalonia.Controls;

namespace Asv.Drones;

[ExportViewFor(typeof(SetupPageViewModel))]
public partial class SetupPageView : UserControl
{
    public SetupPageView()
    {
        InitializeComponent();
    }
}
