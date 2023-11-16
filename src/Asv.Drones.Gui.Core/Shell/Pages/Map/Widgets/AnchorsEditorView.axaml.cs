using System.ComponentModel.Composition;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Core;

[ExportView(typeof(AnchorsEditorViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class AnchorsEditorView : ReactiveUserControl<AnchorsEditorViewModel>
{
    public AnchorsEditorView()
    {
        InitializeComponent();
    }
}