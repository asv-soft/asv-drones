using Asv.Cfg;

namespace Asv.Drones.Gui.Core;

public enum BearingUnits
{
    Degree
}
public class Bearing : MeasureUnitBase<double,BearingUnits>
{
    private static readonly IMeasureUnitItem<double, BearingUnits>[] _units = {
        new DoubleMeasureUnitItem<BearingUnits>(BearingUnits.Degree,RS.BearingDegreeTitle,"°",true,"F2",1)
    };
    
    public Bearing(IConfiguration cfgSvc, string cfgKey) : base(cfgSvc, cfgKey, _units)
    {
    }

    public override string Title => "Bearing";

    public override string Description => "Units of measure for bearing";
}