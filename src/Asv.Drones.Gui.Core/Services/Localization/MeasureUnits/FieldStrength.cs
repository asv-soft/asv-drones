using Asv.Cfg;

namespace Asv.Drones.Gui.Core;


public enum FieldStrengthUnits
{
    MicroVoltsPerMeter
}

public class FieldStrength : MeasureUnitBase<double, FieldStrengthUnits>
{
    private static readonly IMeasureUnitItem<double, FieldStrengthUnits>[] _units = {
        new DoubleMeasureUnitItem<FieldStrengthUnits>(FieldStrengthUnits.MicroVoltsPerMeter, 
            RS.FieldStrength_Title, RS.FieldStrength_MicroVoltsPerMeter_Unit,
            true, "F0", 1)
    };
    
    public FieldStrength(IConfiguration cfgSvc, string cfgKey) : base(cfgSvc, cfgKey, _units)
    {
        
    }

    public override string Title { get; }
    public override string Description { get; }
}