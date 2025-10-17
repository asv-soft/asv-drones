using Asv.Avalonia;
using Avalonia.Controls;

namespace Asv.Drones;

[ExportViewFor(typeof(ParamItemViewModel))]
public partial class ParamItemView : UserControl
{
    public ParamItemView()
    {
        InitializeComponent();
    }
}
