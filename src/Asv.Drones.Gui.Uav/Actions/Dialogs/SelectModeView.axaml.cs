using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Uav;

[ExportView(typeof(SelectModeViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class SelectModeView : ReactiveUserControl<SelectModeViewModel>
{
    public SelectModeView()
    {
        InitializeComponent();
    }
}