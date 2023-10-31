using System.Collections.ObjectModel;

namespace Asv.Drones.Gui.Core
{
    public interface IShellMenuItem : IViewModel
    {
        IShellMenuItem? Parent { get; set; }
        string Name { get; }
        Uri NavigateTo { get; }
        string Icon { get; }
        ShellMenuPosition Position { get; }
        ShellMenuItemType Type { get; }
        int Order { get; }
        ReadOnlyObservableCollection<IShellMenuItem>? Items { get; }
        bool IsSelected { get; set; }
        bool IsVisible { get; set; }
    }

    public enum ShellMenuPosition
    {
        Top,
        Bottom,
    }

    public enum ShellMenuItemType
    {
        Header,
        Group,
        PageNavigation
    }
}
