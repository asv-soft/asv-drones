using Asv.Common;

namespace Asv.Drones.Gui.Airports;

public class AirportModel
{
    public long Id { get; set; }
    public string? Ident { get; set; }
    public string? Name { get; set; }
    public GeoPoint Location { get; set; }
    public string? GPSCode { get; set; }
    public string? LocalCode { get; set; }
}