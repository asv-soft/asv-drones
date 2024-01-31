using System.ComponentModel.Composition;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Core;

[ExportView(typeof(PlaningMissionEditorViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class PlaningMissionEditorView : ReactiveUserControl<PlaningMissionEditorViewModel>
{
    public PlaningMissionEditorView()
    {
        InitializeComponent();
    }
}