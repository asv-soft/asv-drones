using Asv.Cfg;
using Asv.Drones.Gui.Api;

namespace Asv.Drones.Gui;

public class AmplitudeModulation : MeasureUnitBase<double, AmplitudeModulationUnits>
{
    private const double PercentInParts = 0.01;

    private static readonly IMeasureUnitItem<double, AmplitudeModulationUnits>[] _units =
    {
        new DoubleMeasureUnitItem<AmplitudeModulationUnits>(AmplitudeModulationUnits.Percent,
            RS.AmplitudeModulation_Percent_Title, "%", true, "F2", PercentInParts),
        new DoubleMeasureUnitItem<AmplitudeModulationUnits>(AmplitudeModulationUnits.InParts,
            RS.AmplitudeModulation_InParts_Unit, "1", true, "F4", 1),
    };

    public AmplitudeModulation(IConfiguration cfgSvc, string cfgKey) : base(cfgSvc, cfgKey, _units)
    {
    }

    public override string Title => RS.AmplitudeModulation_Title;

    public override string Description => RS.AmplitudeModulation_Description;
}