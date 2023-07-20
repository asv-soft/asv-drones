using System.ComponentModel.Composition;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Core;

[ExportView(typeof(AnchorMoverActionViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class AnchorMoverActionView : ReactiveUserControl<AnchorMoverActionViewModel>
{
    public AnchorMoverActionView()
    {
        InitializeComponent();
    }
}