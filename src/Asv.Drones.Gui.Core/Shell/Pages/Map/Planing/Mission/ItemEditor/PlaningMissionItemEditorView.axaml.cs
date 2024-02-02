using System.ComponentModel.Composition;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Core;


[ExportView(typeof(PlaningMissionItemEditorViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class PlaningMissionItemEditorView : ReactiveUserControl<PlaningMissionItemEditorViewModel>
{
    public PlaningMissionItemEditorView()
    {
        InitializeComponent();
    }
}