using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Gbs;

[ExportView(typeof(SetCoordsNameViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class SetCoordsNameView : ReactiveUserControl<SetCoordsNameViewModel>
{
    public SetCoordsNameView()
    {
        InitializeComponent();
    }
}