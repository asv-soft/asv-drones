using System.Collections.ObjectModel;

namespace Asv.Drones.Gui.Core
{
    public interface IShellMenuItem : IViewModel
    {
        string Name { get; }
        Uri NavigateTo { get; }
        string Icon { get; }
        ShellMenuPosition Position { get; }
        ShellMenuItemType Type { get; }
        int Order { get; }
        ReadOnlyObservableCollection<IShellMenuItem>? Items { get; }
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
