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

        var control = this.FindControl<NumericUpDown>("AltitudeTextBox");
        if (control != null)
        {
            control.AttachedToVisualTree += (s, e) => control.Focus();
        }
    }

    private void Altitude_OnKeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter:
                e.Handled = true;

                var current = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement();
                if (current != null)
                {
                    var next = KeyboardNavigationHandler.GetNext(current, NavigationDirection.Next);
                    
                    while (next != null && (next is RepeatButton || !next.Focusable || !next.IsEnabled))
                    {
                        next = KeyboardNavigationHandler.GetNext(current, NavigationDirection.Next);
                        next?.Focus();
                    }

                }

                break;
        }
    }
}