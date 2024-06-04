using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia.Input;
using Material.Icons;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Api
{
    public class MenuItem : ViewModelBase, IMenuItem
    {
        public MenuItem(Uri id) : base(id)
        {
        }

        public MenuItem(string id) : base(id)
        {
        }

        [Reactive] public int Order { get; set; }
        [Reactive] public MaterialIconKind Icon { get; set; }
        [Reactive] public string Header { get; set; }
        [Reactive] public ICommand Command { get; set; }
        [Reactive] public object? CommandParameter { get; set; }
        [Reactive] public bool IsVisible { get; set; } = true;
        [Reactive] public bool StaysOpenOnClick { get; set; }
        [Reactive] public bool IsEnabled { get; set; } = true;
        public virtual ReadOnlyObservableCollection<IMenuItem>? Items { get; set; }
        [Reactive] public KeyGesture? HotKey { get; set; }
    }
}