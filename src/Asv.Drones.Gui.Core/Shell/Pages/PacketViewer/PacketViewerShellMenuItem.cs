using System.ComponentModel.Composition;
using Material.Icons;

namespace Asv.Drones.Gui.Core;

[Export(typeof(IShellMenuItem))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class PacketViewerShellMenuItem : ShellMenuItem
{
    public PacketViewerShellMenuItem() : base("asv:shell.menu.packets")
    {
        Name = RS.PacketViewerShellMenuItem_Name;
        NavigateTo = PacketViewerViewModel.Uri;
        Icon = MaterialIconDataProvider.GetData(MaterialIconKind.Package);
        Position = ShellMenuPosition.Bottom;
        Type = ShellMenuItemType.PageNavigation;
        Order = -2;
    }
}