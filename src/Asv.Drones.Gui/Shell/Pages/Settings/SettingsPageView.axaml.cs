using Asv.Drones.Gui.Api;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui;

[ExportView(typeof(SettingsPageViewModel))]
public partial class SettingsPageView : ReactiveUserControl<SettingsPageViewModel>
{
    public SettingsPageView()
    {
        InitializeComponent();
    }
}