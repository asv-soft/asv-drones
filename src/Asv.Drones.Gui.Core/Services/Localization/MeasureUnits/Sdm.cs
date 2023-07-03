using Asv.Cfg;

namespace Asv.Drones.Gui.Core;

public enum SdmUnits
{
    Percent,
    InParts
}
public class Sdm : MeasureUnitBase<double,SdmUnits>
{
    private const double PercentInParts = 100.0;

    private static readonly IMeasureUnitItem<double, SdmUnits>[] _units = {
        new DoubleMeasureUnitItem<SdmUnits>(SdmUnits.Percent,RS.Ddm_Percent_Title,"%",true,"F2",1),
        new DoubleMeasureUnitItem<SdmUnits>(SdmUnits.InParts,RS.Ddm_InParts_Title,"1",false, "F4",PercentInParts)
    };
    
    public Sdm(IConfiguration cfgSvc, string cfgKey) : base(cfgSvc, cfgKey, _units)
    {
    }

    public override string Title => "SDM";

    public override string Description => "Units of measure for sum depth of modulation";
}