using Asv.Drones.Gui.Api;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui;

[ExportView(typeof(VoltageUavRttItemViewModel))]
public partial class VoltageUavRttItemView : ReactiveUserControl<VoltageUavRttItemViewModel>
{
    public VoltageUavRttItemView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}