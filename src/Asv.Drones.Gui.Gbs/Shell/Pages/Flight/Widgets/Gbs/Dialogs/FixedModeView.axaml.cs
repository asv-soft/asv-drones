using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Gbs;

[ExportView(typeof(FixedModeViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class FixedModeView : ReactiveUserControl<FixedModeViewModel>
{
    public FixedModeView()
    {
        InitializeComponent();
    }
}