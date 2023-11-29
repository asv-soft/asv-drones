using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Uav;

[ExportView(typeof(RebootAutopilotViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class RebootAutopilotView : ReactiveUserControl<RebootAutopilotViewModel>
{
    public RebootAutopilotView()
    {
        InitializeComponent();
    }
}