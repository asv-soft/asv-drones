using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Sdr;

[ExportView(typeof(MoveDialogViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class MoveDialogView : ReactiveUserControl<MoveDialogViewModel>
{
    public MoveDialogView()
    {
        InitializeComponent();
    }
}