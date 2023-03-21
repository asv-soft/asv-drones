using System.ComponentModel.Composition;
using Material.Icons;

namespace Asv.Drones.Gui.Core;
[Export(typeof(IShellMenuItem))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class ParametersEditorShellMenuItem : ShellMenuItem
{
    
    public const string UriString = ShellMenuItem.UriString + ".parameters";
    public static readonly Uri Uri = new(UriString);
    
    public ParametersEditorShellMenuItem() : base(Uri)
    {
        Name = "Parameters editor";
        NavigateTo = ParametersEditorPageViewModel.Uri;
        Icon = MaterialIconDataProvider.GetData(MaterialIconKind.ViewList);
        Position = ShellMenuPosition.Top;
        Type = ShellMenuItemType.PageNavigation;
        Order = 2;
    }
}