using System.ComponentModel.Composition;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Core;

[ExportView(typeof(RemoveMapPointViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class RemoveMapPointView : ReactiveUserControl<RemoveMapPointViewModel>
{
    public RemoveMapPointView()
    {
        InitializeComponent();
    }
}