using System.Globalization;
using Asv.Cfg;
using Asv.Drones.Gui.Api;
using Avalonia.Controls.Documents;

namespace Asv.Drones.Gui;

public class Distance : MeasureUnitBase<double, DistanceUnits>
{
    private const double MetersInInternationalNauticalMile = 1852;

    private static readonly IMeasureUnitItem<double, DistanceUnits>[] _units =
    {
        new DoubleMeasureUnitItem<DistanceUnits>(DistanceUnits.Meters, RS.Distance_Meters_Title,
            RS.Distance_Meters_Unit, true, "F0", 1),
        new DoubleMeasureUnitItem<DistanceUnits>(DistanceUnits.NauticalMiles, RS.Distance_NauticalMiles_Title,
            RS.Distance_NauticalMiles_Unit, false, "F4", MetersInInternationalNauticalMile),
    };

    public Distance(IConfiguration cfgSvc, string cfgKey) : base(cfgSvc, cfgKey, _units)
    {
    }

    public override string Title => RS.Distance_Title;
    public override string Description => RS.Distance_Description;

    public bool IsValid(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var v);
        return (v > 0);
    }
}