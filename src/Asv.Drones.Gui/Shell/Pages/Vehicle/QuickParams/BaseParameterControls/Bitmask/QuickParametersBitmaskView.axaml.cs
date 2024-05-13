using Asv.Drones.Gui.Api;
using Avalonia.Controls;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui;


[ExportView(typeof(QuickParametersBitmaskViewModel))]
public partial class QuickParametersBitmaskView : ReactiveUserControl<QuickParametersBitmaskViewModel>
{
    public QuickParametersBitmaskView()
    {
        InitializeComponent();
    }
}