using System.ComponentModel.Composition;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Core;

[ExportView(typeof(AddNewMapPointViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class AddNewMapPointView : ReactiveUserControl<AddNewMapPointViewModel>
{
    public AddNewMapPointView()
    {
        InitializeComponent();
    }
}