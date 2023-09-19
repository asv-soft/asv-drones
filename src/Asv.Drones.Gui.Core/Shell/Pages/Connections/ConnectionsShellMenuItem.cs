using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using Material.Icons;

namespace Asv.Drones.Gui.Core
{
    [Export(typeof(IShellMenuItem))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class ConnectionsShellMenuItem : DisposableReactiveObject, IShellMenuItem
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