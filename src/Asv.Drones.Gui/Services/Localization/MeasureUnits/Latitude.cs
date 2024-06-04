using Asv.Cfg;
using Asv.Common;
using Asv.Drones.Gui.Api;

namespace Asv.Drones.Gui;

public class Latitude : MeasureUnitBase<double, LatitudeUnits>
{
    private static readonly IMeasureUnitItem<double, LatitudeUnits>[] Units =
    {
        new LatitudeUnitDeg(),
        new LatitudeUnitDms()
    };

    public Latitude(IConfiguration cfgSvc, string cfgKey) : base(cfgSvc, cfgKey, Units)
    {
    }

    public override string Title => RS.Latitude_Title;
    public override string Description => RS.Latitude_Description;
}

public class LatitudeUnitDeg : IMeasureUnitItem<double, LatitudeUnits>
{
    public LatitudeUnits Id => LatitudeUnits.Deg;
    public string Title => RS.Latitude_Degree_Title;
    public string Unit => RS.Latitude_Degree_Title;
    public bool IsSiUnit => true;

    public double ConvertFromSi(double siValue)
    {
        return siValue;
    }

    public double ConvertToSi(double value)
    {
        return value;
    }

    public double Parse(string? value)
    {
        return value != null && GeoPointLatitude.TryParse(value, out var result) ? result : double.NaN;
    }

    public bool IsValid(string? value)
    {
        return value != null && GeoPointLatitude.IsValid(value);
    }

    public string? GetErrorMessage(string? value)
    {
        return GeoPointLatitude.GetErrorMessage(value);
    }

    public string Print(double value)
    {
        return value.ToString("F6");
    }

    public string PrintWithUnits(double value)
    {
        return $"{value:F6}°";
    }

    public string FromSiToStringWithUnits(double value)
    {
        return $"{value:F6}°";
    }
}

public class LatitudeUnitDms : IMeasureUnitItem<double, LatitudeUnits>
{
    public LatitudeUnits Id => LatitudeUnits.Dms;
    public string Title => RS.Latitude_DMS_Title;
    public string Unit => RS.Latitude_DMS_Title;
    public bool IsSiUnit => false;

    public double ConvertFromSi(double siValue)
    {
        return siValue;
    }

    public double ConvertToSi(double value)
    {
        return value;
    }

    public double Parse(string? value)
    {
        return value != null && GeoPointLatitude.TryParse(value, out var result) ? result : double.NaN;
    }

    public bool IsValid(string? value)
    {
        return value != null && GeoPointLatitude.IsValid(value);
    }

    public string? GetErrorMessage(string? value)
    {
        return GeoPointLatitude.GetErrorMessage(value);
    }

    public string Print(double value)
    {
        return GeoPointLatitude.PrintDms(value);
    }

    public string PrintWithUnits(double value)
    {
        return GeoPointLatitude.PrintDms(value);
    }
}