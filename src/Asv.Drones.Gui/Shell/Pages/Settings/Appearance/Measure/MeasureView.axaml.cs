using Asv.Drones.Gui.Api;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui;

[ExportView(typeof(MeasureViewModel))]
public partial class MeasureView : ReactiveUserControl<MeasureViewModel>
{
    public MeasureView()
    {
        InitializeComponent();
    }
}