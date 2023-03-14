using System.ComponentModel.Composition;
using Avalonia.Controls;
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

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void Control_OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (sender is ScrollViewer sv)
        { 
            sv.ScrollToEnd();   
        }
    }
}