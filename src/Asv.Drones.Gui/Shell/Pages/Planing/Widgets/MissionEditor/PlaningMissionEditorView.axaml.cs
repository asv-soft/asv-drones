using Asv.Drones.Gui.Api;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui;

[ExportView(typeof(PlaningMissionEditorViewModel))]
public partial class PlaningMissionEditorView : ReactiveUserControl<PlaningMissionEditorViewModel>
{
    public PlaningMissionEditorView()
    {
        InitializeComponent();
    }
}