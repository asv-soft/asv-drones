using Asv.Cfg;

namespace Asv.Drones.Gui.Core;

public enum AngleUnits
{
    Degrees,
    DegreesMinutesSeconds
}

public class Angle : MeasureUnitBase<double, AngleUnits>
{
    private static readonly IMeasureUnitItem<double, AngleUnits>[] _units = {
        new DoubleMeasureUnitItem<AngleUnits>(AngleUnits.Degrees,RS.Altitude_Meter_Title,RS.Altitude_Meter_Unit,true, "F0",1),
        new DmsAngleMeasureUnit(),
    };
    
    public Angle(IConfiguration cfgSvc, string cfgKey) : base(cfgSvc, cfgKey, _units)
    {
    }

    public override string Title => "Angle";
    public override string Description => "Units of measure for the angle";
}

public class DmsAngleMeasureUnit : IMeasureUnitItem<double, AngleUnits>
{
    public AngleUnits Id => AngleUnits.DegreesMinutesSeconds;
    public string Title => "[Deg]°[Min]′[Sec]′′";
    public string Unit => "[Deg]°[Min]′[Sec]′′";
    public bool IsSiUnit => false;
    public double ConvertFromSi(double siValue)
    {
        return siValue;
    }

    public double ConvertToSi(double value)
    {
        return value;
    }

    public bool IsValid(string value)
    {
        return Angle.IsValid(value);
    }

    public string? GetErrorMessage(string value)
    {
        return Angle.GetErrorMessage(value);
    }

    public double ConvertToSi(string value)
    {
        return Angle.TryParse(value, out var result) ? result : double.NaN;
    }

    public string FromSiToString(double value)
    {
        return value.ToString("F7");
    }

    public string FromSiToStringWithUnits(double value)
    {
        return $"{value:F7}°";
    }
}