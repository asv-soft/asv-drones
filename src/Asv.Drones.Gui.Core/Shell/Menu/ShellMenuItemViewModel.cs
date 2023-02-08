using System.Collections.ObjectModel;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Core
{
    public class ShellMenuItemViewModel : ViewModelBase, IShellMenuItem
    {
        public ShellMenuItemViewModel(Uri id) : base(id)
        {

        }
        [Reactive]
        public string Name { get; set; }
        public Uri NavigateTo { get; set; }
        [Reactive]
        public string Icon { get; set; }
        public ShellMenuPosition Position { get; set; }
        public ShellMenuItemType Type { get; set; }
        public int Order { get; set; }
        public ReadOnlyObservableCollection<IShellMenuItem>? Items { get; set; }
    }
}