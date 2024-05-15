using Asv.Drones.Gui.Api;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui;

[ExportView(typeof(PlaningMissionItemEditorViewModel))]
public partial class PlaningMissionItemEditorView : ReactiveUserControl<PlaningMissionItemEditorViewModel>
{
    public PlaningMissionItemEditorView()
    {
        InitializeComponent();
    }
}