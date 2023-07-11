using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Sdr;


[ExportView(typeof(VorSdrRttViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class VorSdrRttView : ReactiveUserControl<VorSdrRttViewModel>
{
    public VorSdrRttView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}