using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia.Input;
using Material.Icons;

namespace Asv.Drones.Gui.Core
{
    public interface IHeaderMenuItem:IViewModel
    {
        int Order { get; }
        MaterialIconKind Icon { get; }
        string Header { get; }
        ICommand Command { get; }
        object? CommandParameter { get; }
        bool IsVisible { get; }
        bool StaysOpenOnClick { get; }
        ReadOnlyObservableCollection<IHeaderMenuItem>? Items { get; }
        public bool IsEnabled { get; }
        public KeyGesture? HotKey { get; }
    }
}