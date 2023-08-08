using Asv.Cfg;

namespace Asv.Drones.Gui.Core;

public enum DistanceUnits
{
    Meters,
    NauticalMiles
}
public class Distance : MeasureUnitBase<double,DistanceUnits>
{
    private const double MetersInInternationalNauticalMile = 1852;
    private static readonly IMeasureUnitItem<double, DistanceUnits>[] _units = {
        new DoubleMeasureUnitItem<DistanceUnits>(DistanceUnits.Meters,RS.Distance_Meters_Title,RS.Distance_Meters_Unit,true,"F1",1),
        new DoubleMeasureUnitItem<DistanceUnits>(DistanceUnits.NauticalMiles,RS.Distance_NauticalMiles_Title,RS.Distance_NauticalMiles_Unit,false,"F4",MetersInInternationalNauticalMile),
    };
    public Distance(IConfiguration cfgSvc,string cfgKey):base(cfgSvc,cfgKey,_units)
    {
        
    }
    public override string Title => RS.Distance_Title;
    public override string Description => RS.Distance_Description;

}