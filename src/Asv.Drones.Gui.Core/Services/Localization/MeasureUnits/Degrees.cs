using Asv.Cfg;
using Asv.Common;

namespace Asv.Drones.Gui.Core;

public enum DegreeUnits
{
    Degrees,
    DegreesMinutesSeconds
}

public class Degrees : MeasureUnitBase<double, DegreeUnits>
{
    private static readonly IMeasureUnitItem<double, DegreeUnits>[] _units = {
        new DegreesMeasureUnit(),
        new DmsMeasureUnit()
    };
    
    public Degrees(IConfiguration cfgSvc, string cfgKey) : base(cfgSvc, cfgKey, _units)
    {
    }

    public override string Title => RS.Degrees_Title;
    public override string Description => RS.Degrees_Description;
}

public class DegreesMeasureUnit : IMeasureUnitItem<double, DegreeUnits>
{
    public DegreeUnits Id => DegreeUnits.Degrees;
    public string Title => RS.Degrees_Degree_Title;
    public string Unit => RS.Degrees_Degree_Title;
    public bool IsSiUnit => true;
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
        return value != null && Angle.TryParse(value, out var result) ? result : double.NaN;
    }

    public bool IsValid(string? value)
    {
        return value != null && Angle.IsValid(value);
    }

    public string? GetErrorMessage(string? value)
    {
        return Angle.GetErrorMessage(value);
    }

    public string Print(double value)
    {
        return $"{value}";
    }

    public string PrintWithUnits(double value)
    {
        return $"{value} {Unit}";
    }

}

public class DmsMeasureUnit : IMeasureUnitItem<double, DegreeUnits>
{
    public DegreeUnits Id => DegreeUnits.DegreesMinutesSeconds;
    public string Title => RS.Degrees_DMS_Title;
    public string Unit => RS.Degrees_DMS_Title;
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
        return value != null && Angle.TryParse(value, out var result) ? result : double.NaN;
    }

    public bool IsValid(string? value)
    {
        return value != null && Angle.IsValid(value);
    }

    public string? GetErrorMessage(string? value)
    {
        return Angle.GetErrorMessage(value);
    }

    public string Print(double value)
    {
        return Angle.PrintDms(value);
    }

    public string PrintWithUnits(double value)
    {
        return Angle.PrintDms(value);
    }

}