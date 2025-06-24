using Asv.Avalonia;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Drones.Api;

[ExportViewFor<MavParamComboBoxViewModel>]
public partial class MavParamComboBoxView : UserControl
{
    public MavParamComboBoxView()
    {
        InitializeComponent();
    }
}
