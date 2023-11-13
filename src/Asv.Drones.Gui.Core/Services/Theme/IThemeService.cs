using Asv.Common;
using Avalonia.Styling;

namespace Asv.Drones.Gui.Core
{
    public class ThemeItem
    {
        public ThemeItem(string id, string name,ThemeVariant theme)
        {
            Id = id;
            Name = name;
            Theme = theme;
        }

        public string Id { get; }
        public string Name { get; }
        public ThemeVariant Theme { get; }
    }


    public interface IThemeService
    {
        IEnumerable<ThemeItem> Themes { get; }
        IRxEditableValue<ThemeItem> CurrentTheme { get; }
    }
}