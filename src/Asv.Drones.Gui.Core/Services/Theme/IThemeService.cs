using Asv.Common;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace Asv.Drones.Gui.Core
{
    public class ThemeItem
    {
        public ThemeItem(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public string Id { get; }
        public string Name { get; }
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