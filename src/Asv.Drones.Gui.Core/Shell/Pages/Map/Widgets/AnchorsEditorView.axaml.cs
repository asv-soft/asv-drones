using System.ComponentModel.Composition;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
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