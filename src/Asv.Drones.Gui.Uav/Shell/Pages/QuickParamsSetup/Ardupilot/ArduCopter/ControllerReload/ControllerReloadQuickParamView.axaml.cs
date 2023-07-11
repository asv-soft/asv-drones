using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Uav;


[ExportView(typeof(ControllerReloadQuickParamViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class ControllerReloadQuickParamView : ReactiveUserControl<ControllerReloadQuickParamViewModel>
{
    public ControllerReloadQuickParamView()
    {
        InitializeComponent();
    }
}