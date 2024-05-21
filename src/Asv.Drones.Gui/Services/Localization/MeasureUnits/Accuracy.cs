using Asv.Cfg;
using Asv.Drones.Gui.Api;

namespace Asv.Drones.Gui;

public class Accuracy : MeasureUnitBase<double, DistanceUnits>
{
    private const double MetersInInternationalNauticalMile = 1852;

    private static readonly IMeasureUnitItem<double, DistanceUnits>[] _units =
    {
        new DoubleMeasureUnitItem<DistanceUnits>(DistanceUnits.Meters, RS.Accuracy_Meters_Title,
            RS.Accuracy_Meters_Unit, true, "F2", 1),
        new DoubleMeasureUnitItem<DistanceUnits>(DistanceUnits.NauticalMiles, RS.Accuracy_NauticalMiles_Title,
            RS.Accuracy_NauticalMiles_Unit, false, "F4", MetersInInternationalNauticalMile),
    };

    public Accuracy(IConfiguration cfgSvc, string cfgKey) : base(cfgSvc, cfgKey, _units)
    {
    }

    public override string Title => RS.Accuracy_Title;
    public override string Description => RS.Accuracy_Description;
}