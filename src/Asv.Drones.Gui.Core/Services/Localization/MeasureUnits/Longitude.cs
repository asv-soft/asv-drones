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

    public override string Title => RS.Longitude_Title;
    public override string Description => RS.Longitude_Description;
}

public class LongitudeUnitDeg :  IMeasureUnitItem<double, LongitudeUnits>
{
    public LongitudeUnits Id => LongitudeUnits.Deg;
    public string Title => RS.Longitude_Degree_Title;
    public string Unit => RS.Longitude_Degree_Title;
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
    public string Title => RS.Longitude_DMS_Title;
    public string Unit => RS.Longitude_DMS_Title;
    public bool IsSiUnit => false;
    public double ConvertFromSi(double siValue)
    {
        return siValue;
    }

    public double ConvertToSi(double value)
    {
        return value;
    }

    public bool IsValid(string? value)
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