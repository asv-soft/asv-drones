using System.ComponentModel.Composition;
using Material.Icons;

namespace Asv.Drones.Gui.Core;


[Export(typeof(IShellMenuItem))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class LogsShellMenuItem : ShellMenuItem
{
    
    public const string UriString = ShellMenuItem.UriString + ".logs";
    public static readonly Uri Uri = new(UriString);
    
    public LogsShellMenuItem() : base(Uri)
    {
        Name = RS.LogsShellMenuItem_Name;
        NavigateTo = LogsPageViewModel.Uri;
        Icon = MaterialIconDataProvider.GetData(MaterialIconKind.Journal);
        Position = ShellMenuPosition.Top;
        Type = ShellMenuItemType.PageNavigation;
        Order = 1;
    }
}