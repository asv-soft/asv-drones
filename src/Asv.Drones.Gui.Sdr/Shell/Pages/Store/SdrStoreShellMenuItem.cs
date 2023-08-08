using System.ComponentModel.Composition;
using Asv.Drones.Gui.Core;
using Material.Icons;

namespace Asv.Drones.Gui.Sdr;

[Export(typeof(IShellMenuItem))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class SdrStoreShellMenuItem:ShellMenuItem
{
    public SdrStoreShellMenuItem():base("asv:shell.menu.sdr-store")
    {
        Name = "SDR store";
        NavigateTo = SdrStorePageViewModel.Uri;
        Icon = MaterialIconDataProvider.GetData(MaterialIconKind.DatabaseOutline);
        Position = ShellMenuPosition.Top;
        Type = ShellMenuItemType.PageNavigation;
        Order = 0;
    }
}
