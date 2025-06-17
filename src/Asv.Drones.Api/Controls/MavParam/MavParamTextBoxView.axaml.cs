using Asv.Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace Asv.Drones.Api;

[ExportViewFor(typeof(MavParamTextBoxViewModel))]
public partial class MavParamTextBoxView : UserControl
{
    public MavParamTextBoxView()
    {
        InitializeComponent();
    }

    private void PART_TextBox_OnGotFocus(object? sender, GotFocusEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            textBox.SelectAll();
        }
    }
}
