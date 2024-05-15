using System.Collections.ObjectModel;
using FluentAvalonia.UI.Controls;

namespace Asv.Drones.Gui.Api
{
    public interface IShellMenuItem : IViewModel
    {
        InfoBadge InfoBadge { get; set; }
        IShellMenuItem? Parent { get; set; }
        string Name { get; set; }
        Uri NavigateTo { get; set; }
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


    public interface IShellMenuItem<in TTarget> : IShellMenuItem
    {
        IShellMenuItem Init(TTarget target);
    }
}