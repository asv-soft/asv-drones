using Asv.Cfg;
using Asv.Drones.Gui.Api;

namespace Asv.Drones.Gui;

public class DdmGp : MeasureUnitBase<double, DdmUnits>
{
    private const double AbsoluteInPercent = 0.01;
    private const double AbsoluteInMicroAmp = 0.175 / 150;
    private const double AbsoluteInMicroAmpRu = 0.175 / 250;

    private static readonly IMeasureUnitItem<double, DdmUnits>[] _units =
    {
        new DoubleMeasureUnitItem<DdmUnits>(DdmUnits.InParts, RS.Ddm_InParts_Title, "1", true, "F4", 1),
        new DoubleMeasureUnitItem<DdmUnits>(DdmUnits.Percent, RS.Ddm_Percent_Title, "%", false, "F2",
            AbsoluteInPercent),
        new DoubleMeasureUnitItem<DdmUnits>(DdmUnits.MicroAmp, RS.Ddm_Microamp_Title, RS.Ddm_µA_Unit, false, "F1",
            AbsoluteInMicroAmp),
        new DoubleMeasureUnitItem<DdmUnits>(DdmUnits.MicroAmpRu, RS.Ddm_MicroampRus_Title, RS.Ddm_µA_Unit, false, "F1",
            AbsoluteInMicroAmpRu)
    };

    public DdmGp(IConfiguration cfgSvc, string cfgKey) : base(cfgSvc, cfgKey, _units)
    {
    }

    public override string Title => RS.DdmGp_Title;

    public override string Description => RS.DdmGp_Description;
}