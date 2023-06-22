using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Sdr;

[ExportView(typeof(SdrRttItemStringParamViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class SdrRttItemStringParamView : ReactiveUserControl<SdrRttItemStringParamViewModel>
{
    public SdrRttItemStringParamView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}