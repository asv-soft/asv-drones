using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Uav;


[ExportView(typeof(SpeedsQuickParamViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class SpeedsQuickParamView : ReactiveUserControl<SpeedsQuickParamViewModel>
{
    public SpeedsQuickParamView()
    {
        InitializeComponent();
    }
}