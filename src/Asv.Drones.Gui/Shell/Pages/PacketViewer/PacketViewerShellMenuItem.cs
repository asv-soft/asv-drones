using System.Composition;
using Asv.Drones.Gui.Api;
using Material.Icons;

namespace Asv.Drones.Gui;

[Export(typeof(IShellMenuItem))]
public class PacketViewerShellMenuItem : ShellMenuItem
{
    public PacketViewerShellMenuItem() : base(WellKnownUri.ShellMenuPacketViewer)
    {
        Name = RS.PacketViewerShellMenuItem_Name;
        NavigateTo = WellKnownUri.ShellPagePacketViewerUri;
        Icon = MaterialIconDataProvider.GetData(MaterialIconKind.Package);
        Position = ShellMenuPosition.Bottom;
        Type = ShellMenuItemType.PageNavigation;
        Order = -2;
    }
}