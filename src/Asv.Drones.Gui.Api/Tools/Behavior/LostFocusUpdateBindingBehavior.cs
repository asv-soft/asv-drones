using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;

namespace Asv.Drones.Gui.Api;

public class LostFocusUpdateBindingBehavior : Behavior<TextBox>
{
    static LostFocusUpdateBindingBehavior()
    {
        TextProperty.Changed.Subscribe(e => { ((LostFocusUpdateBindingBehavior)e.Sender).OnBindingValueChanged(); });
    }

    protected override void UpdateDataValidation(AvaloniaProperty property, BindingValueType state, Exception? error)
    {
        base.UpdateDataValidation(property, state, error);
        if (property == TextProperty && AssociatedObject != null)
        {
            if (error != null)
            {
                DataValidationErrors.SetError(AssociatedObject, error);
            }
            else
            {
                DataValidationErrors.ClearErrors(AssociatedObject);
            }
        }
    }


    protected override void OnAttached()
    {
        if (AssociatedObject != null)
        {
            AssociatedObject.LostFocus += OnLostFocus;
            AssociatedObject.KeyDown += OnKeyDown;
        }

        base.OnAttached();
    }

    protected override void OnDetaching()
    {
        if (AssociatedObject != null)
        {
            AssociatedObject.LostFocus -= OnLostFocus;
            AssociatedObject.KeyDown -= OnKeyDown;
        }

        base.OnDetaching();
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (AssociatedObject != null && e.Key == Key.Enter)
        {
            Text = AssociatedObject.Text;
        }
    }

    private void OnLostFocus(object sender, RoutedEventArgs e)
    {
        if (AssociatedObject != null)
            Text = AssociatedObject.Text;
    }

    private void OnBindingValueChanged()
    {
        if (AssociatedObject != null)
            AssociatedObject.Text = Text;
    }

    public static readonly DirectProperty<LostFocusUpdateBindingBehavior, string> TextProperty
        = AvaloniaProperty.RegisterDirect<LostFocusUpdateBindingBehavior, string>(nameof(Text), o => o.Text,
            (o, v) => o.Text = v, null, BindingMode.TwoWay, true);

    private string _text;

    public string Text
    {
        get { return _text; }
        set { this.SetAndRaise(TextProperty, ref _text, value); }
    }
}