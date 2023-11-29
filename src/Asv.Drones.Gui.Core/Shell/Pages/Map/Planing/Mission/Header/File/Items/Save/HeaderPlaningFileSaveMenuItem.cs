using Material.Icons;

namespace Asv.Drones.Gui.Core;

public class HeaderPlaningFileSaveMenuItem : HeaderMenuItem
{
    public const string UriString = HeaderPlaningFileMenuItem.UriString + "/save";
    public static readonly Uri Uri = new(UriString);
    
    public HeaderPlaningFileSaveMenuItem() : base(Uri)
    {
        Header = RS.HeaderPlaningFileSaveMenuItem_Title;
        Icon = MaterialIconKind.ContentSave;
        Order = ushort.MaxValue;
    }
}