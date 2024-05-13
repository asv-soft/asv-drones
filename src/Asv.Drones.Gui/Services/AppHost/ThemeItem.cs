using Asv.Drones.Gui.Api;
using Avalonia.Styling;

namespace Asv.Drones.Gui;

public class ThemeItem : IThemeInfo
{
    /// <summary>
    /// Represents a theme item, which includes an id, name, and theme variant.
    /// </summary>
    /// <param name="id">The unique identifier for the theme item.</param>
    /// <param name="name">The name of the theme item.</param>
    /// <param name="theme">The theme variant for the item.</param>
    public ThemeItem(string id, string name, ThemeVariant theme)
    {
        Id = id;
        Name = name;
        Theme = theme;
    }

    /// <summary>
    /// Gets the identifier of the property.
    /// </summary>
    /// <value>
    /// The identifier of the property.
    /// </value>
    public string Id { get; }

    /// <summary>
    /// Gets the name of the property.
    /// </summary>
    /// <value>
    /// The name of the property.
    /// </value>
    public string Name { get; }

    /// <summary>
    /// Gets the theme variant of the application.
    /// </summary>
    /// <value>
    /// The theme variant of the application.
    /// </value>
    public ThemeVariant Theme { get; }
}