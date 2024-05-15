using Asv.Drones.Gui.Api;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui;

[ExportView(typeof(LoggerViewModel))]
public partial class LoggerView : ReactiveUserControl<LoggerViewModel>
{
    public LoggerView()
    {
        InitializeComponent();
    }
}