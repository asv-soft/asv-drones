﻿using Asv.Cfg;
using Asv.Drones.Gui.Api;

namespace Asv.Drones.Gui;

public class Altitude : MeasureUnitBase<double, AltitudeUnits>
{
    private const double MetersInFeet = 0.3048;

    private static readonly IMeasureUnitItem<double, AltitudeUnits>[] _units =
    {
        new DoubleMeasureUnitItem<AltitudeUnits>(AltitudeUnits.Meters, RS.Altitude_Meter_Title, RS.Altitude_Meter_Unit,
            true, "F2", 1),
        new DoubleMeasureUnitItem<AltitudeUnits>(AltitudeUnits.Feets, RS.Altitude_Feet_Title, RS.Altitude_Feet_Unit,
            false, "F2", MetersInFeet),
    };

    public Altitude(IConfiguration cfgSvc, string cfgKey) : base(cfgSvc, cfgKey, _units)
    {
    }

    public override string Title => RS.Altitude_Title;
    public override string Description => RS.Altitude_Description;
}