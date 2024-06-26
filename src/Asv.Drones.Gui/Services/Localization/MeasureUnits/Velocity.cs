﻿using Asv.Cfg;
using Asv.Drones.Gui.Api;

namespace Asv.Drones.Gui;

public class Velocity : MeasureUnitBase<double, VelocityUnits>
{
    private const double MetersPerSecondInKilometersPerHour = 3.6;

    private const double MetersPerSecondInMilesPerHour = 2.236936;

    private static readonly IMeasureUnitItem<double, VelocityUnits>[] Units =
    {
        new DoubleMeasureUnitItem<VelocityUnits>(VelocityUnits.MetersPerSecond, RS.Velocity_MetersPerSecondUnit,
            RS.Velocity_MetersPerSecondUnit, true, "F0", 1),
        new DoubleMeasureUnitItem<VelocityUnits>(VelocityUnits.KilometersPerHour, RS.Velocity_KilometersPerHourUnit,
            RS.Velocity_KilometersPerHourUnit, false, "F0", MetersPerSecondInKilometersPerHour),
        new DoubleMeasureUnitItem<VelocityUnits>(VelocityUnits.MilesPerHour, RS.Velocity_MilesPerHourUnit,
            RS.Velocity_MilesPerHourUnit, false, "F0", MetersPerSecondInMilesPerHour),
    };

    public Velocity(IConfiguration cfgSvc, string cfgKey) : base(cfgSvc, cfgKey, Units)
    {
    }

    public override string Title => RS.Velocity_Title;
    public override string Description => RS.Velocity_Description;
}