using System.ComponentModel.Composition;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui.Core;

[ExportView(typeof(LoggerViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class LoggerView : ReactiveUserControl<LoggerViewModel>
{
    public LoggerView()
    {
        InitializeComponent();
    }
}