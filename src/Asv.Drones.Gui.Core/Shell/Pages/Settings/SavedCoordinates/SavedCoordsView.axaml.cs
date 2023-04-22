using System.ComponentModel.Composition;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Core;

[ExportView(typeof(SavedCoordsViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class SavedCoordsView : ReactiveUserControl<SavedCoordsViewModel>
{
    public SavedCoordsView()
    {
        InitializeComponent();
    }
}