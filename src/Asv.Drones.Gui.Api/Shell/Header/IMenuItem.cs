using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia.Input;
using Material.Icons;

namespace Asv.Drones.Gui.Api
{
    public interface IMenuItem : IViewModel
    {
        int Order { get; }
        MaterialIconKind Icon { get; }
        string Header { get; }
        ICommand Command { get; }
        object? CommandParameter { get; }
        bool IsVisible { get; }
        bool StaysOpenOnClick { get; }
        ReadOnlyObservableCollection<IMenuItem>? Items { get; set; }
        public bool IsEnabled { get; }
        public KeyGesture? HotKey { get; }
    }
}