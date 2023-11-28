using Asv.Cfg;

namespace Asv.Drones.Gui.Core;

public enum FrequencyUnits
{
    Hz,
    KHz,
    MHz,
    GHz
}
public class Frequency : MeasureUnitBase<double,FrequencyUnits>
{
    
    private const double HzInKHz = 1000;
    private const double HzInMHz = 1000000;
    private const double HzInGHz = 1000000000;

    public static readonly IMeasureUnitItem<double, FrequencyUnits>[] Units = {
        new DoubleMeasureUnitItem<FrequencyUnits>(FrequencyUnits.Hz,RS.Frequency_Hertz_Title,RS.Frequency_Hertz_Unit,true, "F0",1),
        new DoubleMeasureUnitItem<FrequencyUnits>(FrequencyUnits.KHz,RS.Frequency_Kilohertz_Title,RS.Frequency_Kilohertz_Unit,false,"F3",HzInKHz),
        new DoubleMeasureUnitItem<FrequencyUnits>(FrequencyUnits.MHz,RS.Frequency_Megahertz_Title,RS.Frequency_Megahertz_Unit,false,"F3",HzInMHz),
        new DoubleMeasureUnitItem<FrequencyUnits>(FrequencyUnits.GHz,RS.Frequency_Gigahertz_Title,RS.Frequency_Gigahertz_Unit,false,"F3",HzInGHz)
    };
    
    public Frequency(IConfiguration cfgSvc, string cfgKey) : base(cfgSvc, cfgKey, Units)
    {
    }

    public override string Title => RS.Frequency_Title;

    public override string Description => RS.Frequency_Description;
}