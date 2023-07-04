using Asv.Cfg;

namespace Asv.Drones.Gui.Core;

public enum PhaseUnits
{
    Degree,
    Radian
}
public class Phase : MeasureUnitBase<double,PhaseUnits>
{
    private const double DegreeInRadian = 180.0 / Math.PI;

    private static readonly IMeasureUnitItem<double, PhaseUnits>[] _units = {
        new DoubleMeasureUnitItem<PhaseUnits>(PhaseUnits.Degree,RS.Phase_Degree_Title,"°",true,"F2",1),
        new DoubleMeasureUnitItem<PhaseUnits>(PhaseUnits.Radian,RS.Phase_Radian_Title,RS.Phase_Radian_Unit,false, "F2",DegreeInRadian)
    };
    
    public Phase(IConfiguration cfgSvc, string cfgKey) : base(cfgSvc, cfgKey, _units)
    {
    }

    public override string Title => "Phase";

    public override string Description => "Units of measure for phase";
}