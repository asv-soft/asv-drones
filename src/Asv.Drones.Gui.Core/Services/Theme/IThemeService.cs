using Asv.Common;
using Avalonia.Styling;

namespace Asv.Drones.Gui.Core
{
    /// <summary>
    /// Represents a theme item.
    /// </summary>
    public class ThemeItem
    {
        /// <summary>
        /// Represents a theme item, which includes an id, name, and theme variant.
        /// </summary>
        /// <param name="id">The unique identifier for the theme item.</param>
        /// <param name="name">The name of the theme item.</param>
        /// <param name="theme">The theme variant for the item.</param>
        public ThemeItem(string id, string name,ThemeVariant theme)
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


    /// <summary>
    /// Represents a service for managing themes.
    /// </summary>
    public interface IThemeService
    {
        /// <summary>
        /// Gets an enumerable collection of theme items.
        /// </summary>
        /// <value>
        /// An enumerable collection of theme items.
        /// </value>
        IEnumerable<ThemeItem> Themes { get; }

        /// Gets the current theme of the application.
        /// </summary>
        /// <remarks>
        /// This property returns an instance of <see cref="IRxEditableValue{T}"/> which represents the current theme.
        /// </remarks>
        /// <value>
        /// An <see cref="IRxEditableValue{T}"/> instance representing the current theme of the application.
        /// </value>
        IRxEditableValue<ThemeItem> CurrentTheme { get; }
    }
}