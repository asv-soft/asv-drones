using Asv.Drones.Gui.Api;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui;

[ExportView(typeof(ArduPlaneQuickParamStandardTreePageViewModel))]
public partial class ArduPlaneQuickParamStandardTreePageView : ReactiveUserControl<ArduPlaneQuickParamStandardTreePageViewModel>
{
    public ArduPlaneQuickParamStandardTreePageView()
    {
        InitializeComponent();
    }
}