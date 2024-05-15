using Asv.Drones.Gui.Api;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui;

[ExportView(typeof(AppearanceViewModel))]
public partial class AppearanceView : ReactiveUserControl<AppearanceViewModel>
{
    public AppearanceView()
    {
        InitializeComponent();
    }
}