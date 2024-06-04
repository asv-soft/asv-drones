using Avalonia;

namespace Asv.Drones.Gui.Api;

public class StringIndicator : IndicatorBase
{
    #region Directed properties

    private string _title;

    public static readonly DirectProperty<StringIndicator, string> TitleProperty =
        AvaloniaProperty.RegisterDirect<StringIndicator, string>(
            nameof(Title), o => o.Title, (o, v) => o.Title = v);

    public string Title
    {
        get => _title;
        set => SetAndRaise(TitleProperty, ref _title, value);
    }

    private string _value;

    public static readonly DirectProperty<StringIndicator, string> ValueProperty =
        AvaloniaProperty.RegisterDirect<StringIndicator, string>(
            nameof(Value), o => o.Value, (o, v) => o.Value = v);

    public string Value
    {
        get => _value;
        set => SetAndRaise(ValueProperty, ref _value, value);
    }

    #endregion
}