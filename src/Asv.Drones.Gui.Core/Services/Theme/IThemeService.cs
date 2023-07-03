using Asv.Common;
using Avalonia.Media;
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

    public class FlowDirectionItem
    {
        public FlowDirectionItem(FlowDirection id, string name)
        {
            Id = id;
            Name = name;
        }

        public FlowDirection Id { get; }
        public string Name { get; }
    }


    public interface IThemeService
    {
        IEnumerable<ThemeItem> Themes { get; }
        IRxEditableValue<ThemeItem> CurrentTheme { get; }
        IRxEditableValue<FlowDirectionItem> FlowDirection { get; }
        IEnumerable<FlowDirectionItem> FlowDirections { get; }
    }
}