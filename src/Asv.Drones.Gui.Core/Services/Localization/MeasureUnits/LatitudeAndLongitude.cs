using System.Globalization;
using Asv.Cfg;

namespace Asv.Drones.Gui.Core;
public enum LatitudeLongitudeUnits
{
    Degrees,
    DegreesMinutesSeconds
}

public class LatitudeAndLongitudeUnitDegrees:IMeasureUnitItem<double,LatitudeLongitudeUnits>
{
    public LatitudeLongitudeUnits Id => LatitudeLongitudeUnits.Degrees;
    public string Title => RS.MeasureUnitsSettingsViewModel_LatitudeLongtitudeDegrees;
    public string Unit => "°";
    public bool IsSiUnit => true;
    public double ConvertFromSI(double siValue)
    {
        return siValue;
    }

    public double ConvertToSI(double value)
    {
        return value;
    }

    public virtual double ConvertToSI(string value)
    {
        return ConvertToSI(double.Parse(value));
    }

    public string FromSIToString(double value)
    {
        return value.ToString("F5");
    }

    public string FromSIToStringWithUnits(double value)
    {
        return $"{FromSIToString(value)} {Unit}";
    }

    public virtual bool IsValid(string value)
    {
        return double.TryParse(value, out _);
    }

    public virtual string GetErrorMessage(string value)
    {
        value = value.Replace(',', '.');
        return double.TryParse(value, NumberStyles.Any,CultureInfo.InvariantCulture, out var _) == false ? "Value must be a number" : null;
    }
}

public class LatitudeAndLongitudeUnitDegreesMinutesSeconds:IMeasureUnitItem<double,LatitudeLongitudeUnits>
{
    public LatitudeLongitudeUnits Id => LatitudeLongitudeUnits.DegreesMinutesSeconds;
    public string Title => RS.MeasureUnitsSettingsViewModel_LatitudeLongtitudeDegreesMinutesSeconds;
    public string Unit => "";
    public bool IsSiUnit => false;
    public double ConvertFromSI(double siValue)
    {
        return siValue;
    }

    public double ConvertToSI(double value)
    {
        return value;
    }

    public virtual double ConvertToSI(string value)
    {
        return ConvertToSI(double.Parse(value));
    }

    public string FromSIToString(double value)
    {
        var _minutes = (value - (int)value) * 60;
        return $"{value:F0}°{_minutes:F0}'{(_minutes - (int)_minutes) * 60:F2}''";
    }

    public string FromSIToStringWithUnits(double value)
    {
        return FromSIToString(value);
    }

    public virtual bool IsValid(string value)
    {
        return double.TryParse(value, out _);
    }

    public virtual string GetErrorMessage(string value)
    {
        return double.TryParse(value, out _) == false ? "Value must be a number" : null;
    }
}


public class LatitudeAndLongitude : MeasureUnitBase<double,LatitudeLongitudeUnits>
{
    private static readonly IMeasureUnitItem<double, LatitudeLongitudeUnits>[] _units = {
        new LatitudeAndLongitudeUnitDegrees(),
        new LatitudeAndLongitudeUnitDegreesMinutesSeconds()
    };
    public LatitudeAndLongitude(IConfiguration cfgSvc,string cfgKey):base(cfgSvc,cfgKey,_units)
    {
        
    }

  
    public override string Title => "Latitude and Longitude";
    public override string Description => "Units of latitude and longitude";
}