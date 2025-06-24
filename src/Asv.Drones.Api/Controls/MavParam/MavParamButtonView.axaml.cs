using Asv.Avalonia;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Drones.Api;

[ExportViewFor<MavParamButtonViewModel>]
public partial class MavParamButtonView : UserControl
{
    public MavParamButtonView()
    {
        InitializeComponent();
    }
}
