using Asv.Drones.Gui.Api;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui;

[ExportView(typeof(MissionStatusViewModel))]
public partial class MissionStatusView : ReactiveUserControl<MissionStatusViewModel>
{
    public MissionStatusView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}