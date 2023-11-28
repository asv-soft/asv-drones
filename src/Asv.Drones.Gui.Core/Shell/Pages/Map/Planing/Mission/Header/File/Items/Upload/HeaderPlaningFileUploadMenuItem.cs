using Material.Icons;

namespace Asv.Drones.Gui.Core;

public class HeaderPlaningFileUploadMenuItem : HeaderMenuItem
{
    public const string UriString = HeaderPlaningFileMenuItem.UriString + "/upload";
    public static readonly Uri Uri = new(UriString);
    
    public HeaderPlaningFileUploadMenuItem() : base(Uri)
    {
        Header = RS.HeaderPlaningFileUploadMenuItem_Title;
        Icon = MaterialIconKind.Upload;
        Order = ushort.MaxValue;
    }
}