using Asv.Cfg;

namespace Asv.Drones.Gui.Core;

public enum PowerUnits
{
    Dbm
}
public class Power : MeasureUnitBase<double,PowerUnits>
{
    private static readonly IMeasureUnitItem<double, PowerUnits>[] _units = {
        new DoubleMeasureUnitItem<PowerUnits>(PowerUnits.Dbm,RS.Power_Dbm_Title,RS.Power_Dbm_Unit,true,"F2",1),
    };
    
    public Power(IConfiguration cfgSvc, string cfgKey) : base(cfgSvc, cfgKey, _units)
    {
    }

    public override string Title => "Power";

    public override string Description => "Units of measure for power";
}