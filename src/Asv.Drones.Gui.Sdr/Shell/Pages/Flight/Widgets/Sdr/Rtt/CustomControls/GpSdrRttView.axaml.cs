using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Sdr;

[ExportView(typeof(GpSdrRttViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class GpSdrRttView : ReactiveUserControl<GpSdrRttViewModel>
{
    public GpSdrRttView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}