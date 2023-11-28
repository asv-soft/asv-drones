using Material.Icons;

namespace Asv.Drones.Gui.Core;

public class HeaderPlaningFileDeleteMenuItem : HeaderMenuItem
{
    public const string UriString = HeaderPlaningFileMenuItem.UriString + "/delete";
    public static readonly Uri Uri = new(UriString);
    
    public HeaderPlaningFileDeleteMenuItem() : base(Uri)
    {
        Header = RS.HeaderPlaningFileDeleteMenuItem_Title;
        Icon = MaterialIconKind.Delete;
        Order = ushort.MaxValue;
    }
}