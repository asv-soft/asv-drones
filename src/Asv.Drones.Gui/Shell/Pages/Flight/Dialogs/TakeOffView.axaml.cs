using Asv.Drones.Gui.Api;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.ReactiveUI;

namespace Asv.Drones.Gui;

[ExportView(typeof(TakeOffViewModel))]
public partial class TakeOffView : ReactiveUserControl<TakeOffViewModel>
{
    public TakeOffView()
    {
        InitializeComponent();

        var textBox = this.FindControl<TextBox>("AltitudeTextBox");
        if (textBox != null)
        {
            textBox.AttachedToVisualTree += (s, e) => textBox.Focus();
            textBox.AttachedToVisualTree += (s, e) => textBox.SelectAll();
        }
    }

    private void Altitude_OnKeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter:
                e.Handled = true;

                var current = TopLevel.GetTopLevel(this).FocusManager.GetFocusedElement();
                if (current != null)
                {
                    var next = KeyboardNavigationHandler.GetNext(current, NavigationDirection.Next);
                    if (next != null && next.Focusable && next.IsEnabled)
                    {
                        next.Focus();
                    }
                }

                break;
        }
    }
}