using Asv.Cfg;
using Asv.Drones.Gui.Api;

namespace Asv.Drones.Gui;

public class Power : MeasureUnitBase<double, PowerUnits>
{
    public static readonly IMeasureUnitItem<double, PowerUnits>[] Units =
    {
        new DoubleMeasureUnitItem<PowerUnits>(PowerUnits.Dbm, RS.Power_Dbm_Title, RS.Power_Dbm_Unit, true, "F2", 1),
    };

    public Power(IConfiguration cfgSvc, string cfgKey) : base(cfgSvc, cfgKey, Units)
    {
    }

    public override string Title => RS.Power_Title;

    public override string Description => RS.Power_Description;
}