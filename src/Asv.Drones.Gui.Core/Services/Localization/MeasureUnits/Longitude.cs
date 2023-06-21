using Asv.Cfg;
using Asv.Common;

namespace Asv.Drones.Gui.Core;

public enum LongitudeUnits
{
    Deg,
    Dms
}
public class Longitude : MeasureUnitBase<double,LongitudeUnits>
{
    private static readonly IMeasureUnitItem<double, LongitudeUnits>[] Units = {
        new LongitudeUnitDeg(),
        new LongitudeUnitDms()
    };
    public Longitude(IConfiguration cfgSvc,string cfgKey):base(cfgSvc,cfgKey,Units)
    {
        
    }

    public override string Title => "Longitude";
    public override string Description => "Units of longitude";
}

public class LongitudeUnitDeg :  IMeasureUnitItem<double, LongitudeUnits>
{
    public LongitudeUnits Id => LongitudeUnits.Deg;
    public string Title => "[Deg]°";
    public string Unit => "°";
    public bool IsSiUnit => true;
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
        return GeoPointLongitude.IsValid(value);
    }

    public string? GetErrorMessage(string value)
    {
        return GeoPointLongitude.GetErrorMessage(value);
    }

    public double ConvertToSi(string value)
    {
        return GeoPointLongitude.TryParse(value, out var result) ? result : double.NaN;
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

public class LongitudeUnitDms : IMeasureUnitItem<double, LongitudeUnits>
{
    public LongitudeUnits Id => LongitudeUnits.Dms; 
    public string Title => "[Deg]°[Min]′[Sec]′′";
    public string Unit => "°";
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
        return GeoPointLongitude.IsValid(value);
    }

    public string? GetErrorMessage(string value)
    {
        return GeoPointLongitude.GetErrorMessage(value);
    }

    public double ConvertToSi(string value)
    {
        return GeoPointLongitude.TryParse(value, out var result) ? result : double.NaN;
    }

    public string FromSiToString(double value)
    {
        return GeoPointLongitude.PrintDms(value);
    }

    public string FromSiToStringWithUnits(double value)
    {
        return FromSiToString(value);
    }
}