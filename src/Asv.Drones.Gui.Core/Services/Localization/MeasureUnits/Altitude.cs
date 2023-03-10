using Asv.Cfg;

namespace Asv.Drones.Gui.Core;


public enum AltitudeUnits
{
    Meters,
    Feets
}

public class Altitude : MeasureUnitBase<double,AltitudeUnits>
{
    private const double MetersInFeet = 0.3048;

    private static readonly IMeasureUnitItem<double, AltitudeUnits>[] _units = {
        new DoubleMeasureUnitItem<AltitudeUnits>(AltitudeUnits.Meters,RS.Altitude_Meter_Title,RS.Altitude_Meter_Unit,true, "F0",1),
        new DoubleMeasureUnitItem<AltitudeUnits>(AltitudeUnits.Feets,RS.Altitude_Feet_Title,RS.Altitude_Feet_Unit,false,"F0",MetersInFeet),
    };
    public Altitude(IConfiguration cfgSvc, string cfgKey) : base(cfgSvc, cfgKey,_units)
    {
        
    }

    public override string Title => "Altitude";
    public override string Description => "Units of measure for the altitude";
}
