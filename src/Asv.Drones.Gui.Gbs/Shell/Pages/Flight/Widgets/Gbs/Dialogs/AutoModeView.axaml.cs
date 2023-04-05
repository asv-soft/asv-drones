using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Gbs;

[ExportView(typeof(AutoModeViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class AutoModeView : ReactiveUserControl<AutoModeViewModel>
{
    public AutoModeView()
    {
        InitializeComponent();
    }
}