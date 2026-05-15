using Avalonia.Controls;
using Avalonia.Interactivity;
using R3;

namespace Asv.Drones.Api;

public partial class MavParamTextBoxView : UserControl
{
    public MavParamTextBoxView()
    {
        InitializeComponent();
    }

    private void PART_TextBox_OnGotFocus(object? sender, RoutedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            Observable.TimerFrame(1).Subscribe(_ => textBox.SelectAll());
        }
    }
}
