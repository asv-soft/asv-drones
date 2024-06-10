using Asv.Drones.Gui.Api;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui;

[ExportView(typeof(PlaningMissionSavingBrowserViewModel))]
public partial class PlaningMissionSavingBrowserView : ReactiveUserControl<PlaningMissionSavingBrowserViewModel>
{
    public PlaningMissionSavingBrowserView()
    {
        InitializeComponent();
    }
}