using Asv.Cfg;
using Asv.Drones.Gui.Api;

namespace Asv.Drones.Gui;

public class Sdm : MeasureUnitBase<double, SdmUnits>
{
    private const double PercentInParts = 0.01;

    private static readonly IMeasureUnitItem<double, SdmUnits>[] _units =
    {
        new DoubleMeasureUnitItem<SdmUnits>(SdmUnits.Percent, RS.Ddm_Percent_Title, "%", true, "F2", PercentInParts)
    };

    public Sdm(IConfiguration cfgSvc, string cfgKey) : base(cfgSvc, cfgKey, _units)
    {
    }

    public override string Title => RS.Sdm_Title;

    public override string Description => RS.Sdm_Description;
}