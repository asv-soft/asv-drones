using Material.Icons;

namespace Asv.Drones.Gui.Core;

public class HeaderPlaningFileDownloadMenuItem : HeaderMenuItem
{
    public const string UriString = HeaderPlaningFileMenuItem.UriString + "/download";
    public static readonly Uri Uri = new(UriString);
    
    public HeaderPlaningFileDownloadMenuItem() : base(Uri)
    {
        Header = RS.HeaderPlaningFileDownloadMenuItem_Title;
        Icon = MaterialIconKind.Download;
        Order = ushort.MaxValue;
    }
}