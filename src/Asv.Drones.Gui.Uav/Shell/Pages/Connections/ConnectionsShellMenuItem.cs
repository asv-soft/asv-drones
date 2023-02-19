using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Material.Icons;

namespace Asv.Drones.Gui.Uav
{
    [Export(typeof(IShellMenuItem))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class ConnectionsShellMenuItem : DisposableViewModelBase, IShellMenuItem
    {
        public Uri Id { get; } = new("asv:shell.menu.connections");
        public string Name => RS.ConnectionsShellMenuItem_Name;
        public Uri NavigateTo => ConnectionsViewModel.BaseUri;
        public string Icon => MaterialIconDataProvider.GetData(MaterialIconKind.Lan);
        public ShellMenuPosition Position => ShellMenuPosition.Bottom;
        public ShellMenuItemType Type => ShellMenuItemType.PageNavigation;
        public int Order => -1;
        public ReadOnlyObservableCollection<IShellMenuItem>? Items => null;
    }
}