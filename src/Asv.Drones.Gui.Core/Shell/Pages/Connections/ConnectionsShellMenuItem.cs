using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using Avalonia.Media;
using Material.Icons;

namespace Asv.Drones.Gui.Core
{
    [Export(typeof(IShellMenuItem))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class ConnectionsShellMenuItem : ShellMenuItem
    {
        public ConnectionsShellMenuItem():base("asv:shell.menu.connections")
        {
            Name = RS.ConnectionsShellMenuItem_Name;
            NavigateTo = ConnectionsViewModel.BaseUri;
            Icon = MaterialIconDataProvider.GetData(MaterialIconKind.Lan);
            Position = ShellMenuPosition.Bottom;
            Type = ShellMenuItemType.PageNavigation;
            Order = -1;
        }
      
    }
}