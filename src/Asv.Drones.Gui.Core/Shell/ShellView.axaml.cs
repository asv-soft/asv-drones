using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace Asv.Drones.Gui.Core;

[ExportView(typeof(ShellViewModel))]
public partial class ShellView : UserControl
{
    public ShellView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void InputElement_OnTapped(object? sender, TappedEventArgs e)
    {
        
    }

    private void OnAttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e)
    {
        var vm = this.DataContext as ShellViewModel;
        vm?.OnLoaded();
    }
}