using Asv.Drones.Gui.Api;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui;

[ExportView(typeof(ArduCopterQuickParamStandardTreePageViewModel))]
public partial class ArduCopterQuickParamStandardTreePageView : ReactiveUserControl<ArduCopterQuickParamStandardTreePageViewModel>
{
    public ArduCopterQuickParamStandardTreePageView()
    {
        InitializeComponent();
    }
}