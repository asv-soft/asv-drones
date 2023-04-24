using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Sdr;

[ExportView(typeof(SdrRttItemLlzViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class SdrRttItemLlzView : ReactiveUserControl<SdrRttItemLlzViewModel>
{
    public SdrRttItemLlzView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}