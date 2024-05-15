using Asv.Drones.Gui.Api;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui;

[ExportView(typeof(QuickParametersComboBoxViewModel))]
public partial class QuickParametersComboBoxView : ReactiveUserControl<QuickParametersComboBoxViewModel>
{
    public QuickParametersComboBoxView()
    {
        InitializeComponent();
    }
}