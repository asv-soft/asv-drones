using System.Collections.ObjectModel;
using FluentAvalonia.UI.Controls;

namespace Asv.Drones.Gui.Core
{
    /// <summary>
    /// Interface for the ShellMenuItem.
    /// </summary>
    public interface IShellMenuItem : IViewModel
    {
        /// <summary>
        /// Information badge displayed next to the menu item.
        /// </summary>
        InfoBadge InfoBadge { get; set; }
        
        /// <summary>
        /// Parent menu item of the current menu item.
        /// </summary>
        IShellMenuItem? Parent { get; set; }
        
        /// <summary>
        /// Name of the menu item.
        /// </summary>
        string Name { get; set; }
        
        /// <summary>
        /// Navigation target URI of the menu item.
        /// </summary>
        Uri NavigateTo { get; set; }
        
        /// <summary>
        /// Icon of the menu item.
        /// </summary>
        string Icon { get; }
        
        /// <summary>
        /// Position of the menu item in the shell menu.
        /// </summary>
        ShellMenuPosition Position { get; }
        
        /// <summary>
        /// Type of the menu item.
        /// </summary>
        ShellMenuItemType Type { get; }
        
        /// <summary>
        /// Order of the menu item in the shell menu.
        /// </summary>
        int Order { get; }
        
        /// <summary>
        /// Child menu items of the current menu item.
        /// </summary>
        ReadOnlyObservableCollection<IShellMenuItem>? Items { get; }
        
        /// <summary>
        /// indicates if the menu item is currently selected.
        /// </summary>
        bool IsSelected { get; set; }
        
        /// <summary>
        /// indicates if the menu item is currently visible.
        /// </summary>
        bool IsVisible { get; set; }
    }

    /// <summary>
    /// Enum for the position of the ShellMenu item.
    /// </summary>
    public enum ShellMenuPosition
    {
        Top,
        Bottom,
    }

    /// <summary>
    /// Enum for the type of the ShellMenu item.
    /// </summary>
    public enum ShellMenuItemType
    {
        Header,
        Group,
        PageNavigation
    }
    
    /// <summary>
    /// Interface for the ShellMenuItem with a target.
    /// </summary>
    public interface IShellMenuItem<in TTarget>:IShellMenuItem
    {
        /// <summary>
        /// Method to Initialize a ShellMenuItem with a target.
        /// </summary>
        IShellMenuItem Init(TTarget target);
    }
}
