#nullable enable
using System.Collections.ObjectModel;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Core
{
    public class ShellMenuItem : ViewModelBase, IShellMenuItem
    {
        public ShellMenuItem(Uri id) : base(id)
        {
            
        }
        public ShellMenuItem(string id) : base(id)
        {
            
        }
        [Reactive]
        public string Name { get; init; }
        public Uri NavigateTo { get; init; }
        [Reactive]
        public string Icon { get; init; }
        public ShellMenuPosition Position { get; init; }
        public ShellMenuItemType Type { get; init; }
        public int Order { get; init; }
        public ReadOnlyObservableCollection<IShellMenuItem>? Items { get; set; }
    }
}