using Avalonia.Controls;

namespace Asv.Drones.Gui;

public partial class LogViewerView : UserControl
{
    public LogViewerView()
    {
        InitializeComponent();
    }

    private void ListBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is LogViewerViewModel viewModel)
        {
            viewModel.UpdatePage();
        }
    }
}