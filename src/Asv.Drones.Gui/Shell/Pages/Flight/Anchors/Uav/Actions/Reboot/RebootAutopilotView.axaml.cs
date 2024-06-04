using Asv.Drones.Gui.Api;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui;

[ExportView(typeof(RebootAutopilotViewModel))]
public partial class RebootAutopilotView : ReactiveUserControl<RebootAutopilotViewModel>
{
    public RebootAutopilotView()
    {
        InitializeComponent();
    }
}