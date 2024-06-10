using Asv.Drones.Gui.Api;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui;

[ExportView(typeof(PlaningMissionBrowserViewModel))]
public partial class PlaningMissionBrowserView : ReactiveUserControl<PlaningMissionBrowserViewModel>
{
    public PlaningMissionBrowserView()
    {
        InitializeComponent();
    }
}