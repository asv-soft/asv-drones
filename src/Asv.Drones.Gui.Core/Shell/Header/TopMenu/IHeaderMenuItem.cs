using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia.Input;
using Material.Icons;

namespace Asv.Drones.Gui.Core
{
    /// <summary>
    /// This interface represents each Menu Item that is present in the user interface header bar. 
    /// </summary>
    public interface IHeaderMenuItem : IViewModel
    {
        /// <summary>
        /// This property holds the sequence order of the menu item
        /// </summary>
        int Order { get; }
        
        /// <summary>
        /// Icon property that holds the icon of the header menu item
        /// </summary>
        MaterialIconKind Icon { get; }
        
        /// <summary>
        /// Text associated with the menu item in the header
        /// </summary>
        string Header { get; }
        
        /// <summary>
        /// ICommand that will be associated with the activity of the particular header menu item
        /// </summary>
        ICommand Command { get; }

        /// <summary>
        /// Any additional parameter to pass to the command associated with the menu item
        /// </summary>
        object? CommandParameter { get; }
        
        /// <summary>
        /// Property to set the visibility of the menu item in the header
        /// </summary>
        bool IsVisible { get; }
        
        /// <summary>
        /// Determines whether the menu item remains open post a click
        /// </summary>
        bool StaysOpenOnClick { get; }
        
        /// <summary>
        /// Collection of all child items (menu item) if any for the particular header menu item
        /// </summary>
        ReadOnlyObservableCollection<IHeaderMenuItem>? Items { get; set; }
        
        /// <summary>
        /// Property to enable or disable the header menu item
        /// </summary>
        public bool IsEnabled { get; }
        
        /// <summary>
        /// Typically would be the shortcut for the menu item in the header
        /// </summary>
        public KeyGesture? HotKey { get; }
    }
}