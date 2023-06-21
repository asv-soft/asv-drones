using System.ComponentModel.Composition;
using Material.Icons;

namespace Asv.Drones.Gui.Core;


[Export(typeof(IShellMenuItem))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class LogMessagesShellMenuItem : ShellMenuItem
{
    public LogMessagesShellMenuItem() : base("asv:shell.menu.logs")
    {
        Name = RS.LogMessagesShellMenuItem_Name;
        NavigateTo = LogMessagesPageViewModel.Uri;
        Icon = MaterialIconDataProvider.GetData(MaterialIconKind.Journal);
        Position = ShellMenuPosition.Bottom;
        Type = ShellMenuItemType.PageNavigation;
        Order = 1;
    }
}