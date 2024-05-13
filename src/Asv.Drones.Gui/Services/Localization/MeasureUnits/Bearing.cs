using Asv.Cfg;
using Asv.Common;
using Asv.Drones.Gui.Api;

namespace Asv.Drones.Gui;

public class Bearing : MeasureUnitBase<double, BearingUnits>
{
    private static readonly IMeasureUnitItem<double, BearingUnits>[] _units =
    {
        new DoubleMeasureUnitItem<BearingUnits>(BearingUnits.Degree, RS.BearingDegreeTitle, "°", true, "F2", 1),
        new DmMeasureUnit()
    };

    public Bearing(IConfiguration cfgSvc, string cfgKey) : base(cfgSvc, cfgKey, _units)
    {
    }

    public override string Title => RS.Bearing_Title;

    public override string Description => RS.Bearing_Description;
}

public class DmMeasureUnit : IMeasureUnitItem<double, BearingUnits>
{
    public BearingUnits Id => BearingUnits.DegreesMinutes;
    public string Title => RS.Degrees_DM_Title;
    public string Unit => RS.Degrees_DM_Title;

    public bool IsSiUnit => false;

    //Not usable
    public double ConvertFromSi(double siValue)
    {
        return siValue;
    }

    //Not usable
    public double ConvertToSi(double value)
    {
        return value;
    }

    public double Parse(string? value)
    {
        return value != null && AngleDm.TryParse(value, out var result) ? result : double.NaN;
    }

    public bool IsValid(string? value)
    {
        return value != null && AngleDm.IsValid(value);
    }

    public string? GetErrorMessage(string? value)
    {
        return AngleDm.GetErrorMessage(value);
    }

    public string Print(double value)
    {
        return AngleDm.PrintDm(value);
    }

    public string PrintWithUnits(double value)
    {
        return Print(value);
    }
}