using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Sdr;

[ExportView(typeof(LlzFlightSdrViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class LlzFlightSdrView : ReactiveUserControl<LlzFlightSdrViewModel>
{
    public LlzFlightSdrView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}