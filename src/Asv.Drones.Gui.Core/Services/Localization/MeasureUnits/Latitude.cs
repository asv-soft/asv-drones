using Asv.Cfg;
using Asv.Common;

namespace Asv.Drones.Gui.Core;

public enum LatitudeUnits
{
    Deg,
    Dms
}

public class Latitude : MeasureUnitBase<double,LatitudeUnits>
{
    private static readonly IMeasureUnitItem<double, LatitudeUnits>[] Units = {
        new LatitudeUnitDeg(),
        new LatitudeUnitDms()
    };
    public Latitude(IConfiguration cfgSvc,string cfgKey):base(cfgSvc,cfgKey,Units)
    {
        
    }

    public override string Title => "Latitude";
    public override string Description => "Units of latitude";
}

public class LatitudeUnitDeg : IMeasureUnitItem<double, LatitudeUnits>
{
    public LatitudeUnits Id => LatitudeUnits.Deg;
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
        return GeoPointLatitude.IsValid(value);
    }

    public string? GetErrorMessage(string value)
    {
        return GeoPointLatitude.GetErrorMessage(value);
    }

    public double ConvertToSi(string value)
    {
        return GeoPointLatitude.TryParse(value, out var result) ? result : double.NaN;
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

public class LatitudeUnitDms : IMeasureUnitItem<double, LatitudeUnits>
{
    public LatitudeUnits Id => LatitudeUnits.Dms;
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
        return GeoPointLatitude.IsValid(value);
    }

    public string? GetErrorMessage(string value)
    {
        return GeoPointLatitude.GetErrorMessage(value);
    }

    public double ConvertToSi(string value)
    {
        return GeoPointLatitude.TryParse(value, out var result) ? result : double.NaN;
    }

    public string FromSiToString(double value)
    {
        return GeoPointLatitude.PrintDms(value);
    }

    public string FromSiToStringWithUnits(double value)
    {
        return FromSiToString(value);
    }
}