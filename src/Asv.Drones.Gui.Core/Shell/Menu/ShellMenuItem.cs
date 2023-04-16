using System.Collections.ObjectModel;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Core
{
    public class ShellMenuItem : ViewModelBase, IShellMenuItem
    {
        public const string UriString = ShellViewModel.UriString + ".menu";
        public static readonly Uri Uri = new(UriString);

        public ShellMenuItem(Uri id) : base(id)
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