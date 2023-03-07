using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using static System.Net.Mime.MediaTypeNames;

namespace Asv.Drones.Gui.Uav;

[ExportView(typeof(TakeOffViewModel))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public partial class TakeOffView : ReactiveUserControl<TakeOffViewModel>
{
    public TakeOffView()
    {
        InitializeComponent();

        var textBox = this.FindControl<TextBox>("AltitudeTextBox");
        if (textBox != null)
        {
            textBox.AttachedToVisualTree += (s,e) => textBox.Focus();
            textBox.AttachedToVisualTree += (s, e) => textBox.SelectAll();
        }

    }

    private void Altitude_OnKeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter:
                e.Handled = true;

                var current = FocusManager.Instance?.Current;
                if (current != null)
                {
                    var next = KeyboardNavigationHandler.GetNext(current, NavigationDirection.Next);
                    if (next != null && next.Focusable && next.IsEnabled)
                    {
                        FocusManager.Instance?.Focus(next, NavigationMethod.Directional);
                    }
                }
                
                break;
        }
    }
}