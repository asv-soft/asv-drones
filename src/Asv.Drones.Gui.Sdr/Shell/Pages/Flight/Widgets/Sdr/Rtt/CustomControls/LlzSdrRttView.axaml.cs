using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Sdr;

[ExportView(typeof(LlzSdrRttViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class LlzSdrRttView : ReactiveUserControl<LlzSdrRttViewModel>
{
    public LlzSdrRttView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}