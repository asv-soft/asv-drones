using Material.Icons;

namespace Asv.Drones.Gui.Core;

public class HeaderPlaningFileOpenMenuItem : HeaderMenuItem
{
    public const string UriString = HeaderPlaningFileMenuItem.UriString + "/open";
    public static readonly Uri Uri = new(UriString);
    
    public HeaderPlaningFileOpenMenuItem() : base(Uri)
    {
        Header = RS.HeaderPlaningFileOpenMenuItem_Title;
        Icon = MaterialIconKind.FolderOpen;
        Order = ushort.MaxValue;
    }
}