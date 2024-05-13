using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Api;

[ExportView(typeof(AnchorsEditorViewModel))]
public partial class AnchorsEditorView : ReactiveUserControl<AnchorsEditorViewModel>
{
    public AnchorsEditorView()
    {
        InitializeComponent();
    }
}