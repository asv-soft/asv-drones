using System.Globalization;
using Asv.Cfg;
using Asv.Common;
using Asv.Drones.Gui.Api;

namespace Asv.Drones.Gui;

public class Distance : MeasureUnitBase<double, DistanceUnits>
{
    private static readonly IMeasureUnitItem<double, DistanceUnits>[] _units =
    {
        new DistanceMeasureUnitMeters(),
        new DistanceMeasureUnitNauticalMiles()
    };

    public Distance(IConfiguration cfgSvc, string cfgKey) : base(cfgSvc, cfgKey, _units)
    {
    }

    public override string Title => RS.Distance_Title;
    public override string Description => RS.Distance_Description;
}
public class DistanceMeasureUnitMeters : IMeasureUnitItem<double, DistanceUnits>
{
    public DistanceUnits Id => DistanceUnits.Meters;
    public string Title { get; } = RS.Distance_Meters_Title;
    public string Unit { get; } = RS.Distance_Meters_Unit;
    public bool IsSiUnit { get; } = true;
    
    
    public double ConvertFromSi(double siValue)
    {
        return siValue;
    }
    
    public double ConvertToSi(double value)
    {
        return value;
    }
    
    //Not usable
    public double Parse(string? value)
    {
        return value != null && double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : double.NaN;
    }

    public bool IsValid(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var v);
        return v >= 0;
    }
    
    public string? GetErrorMessage(string? value)
    {
        if (value.IsNullOrWhiteSpace()) return Api.RS.DistanceMeasureUnit_ErrorMessage_NullOrWhiteSpace;
        if (!IsValid(value)) return Api.RS.DistanceMeasureUnit_ErrorMessage_NotValid;
        value = value.Replace(',', '.');
        return double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out _) == false
            ? Api.RS.DistanceMeasureUnit_ErrorMessage_NotANumber
            : null;
    }
    
    public string Print(double value)
    {
        return $"{value:F0}";
    }
    
    public string PrintWithUnits(double value)
    {
        return $"{value:F0} {Unit}";
    }
}
public class DistanceMeasureUnitNauticalMiles : IMeasureUnitItem<double, DistanceUnits>
{
    private const double MetersInInternationalNauticalMile = 1852;
    public DistanceUnits Id => DistanceUnits.NauticalMiles;
    public string Title { get; } = RS.Distance_NauticalMiles_Title;
    public string Unit { get; } = RS.Distance_NauticalMiles_Unit;
    public bool IsSiUnit { get; } = false;
    
    public double ConvertFromSi(double siValue)
    {
        return siValue / MetersInInternationalNauticalMile;
    }
    
    public double ConvertToSi(double value)
    {
        return value * MetersInInternationalNauticalMile;
    }
    
    //Not usable
    public double Parse(string? value)
    {
        return value != null && double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : double.NaN;
    }

    public bool IsValid(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var v);
        return v >= 0;
    }
    
    public string? GetErrorMessage(string? value)
    {
        if (value.IsNullOrWhiteSpace()) return Api.RS.DistanceMeasureUnit_ErrorMessage_NullOrWhiteSpace;
        if (!IsValid(value)) return Api.RS.DistanceMeasureUnit_ErrorMessage_NotValid;
        value = value.Replace(',', '.');
        return double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out _) == false
            ? Api.RS.DistanceMeasureUnit_ErrorMessage_NotANumber
            : null;
    }
    
    public string Print(double value)
    {
        return $"{value:F4}";
    }
    
    public string PrintWithUnits(double value)
    {
        return $"{value:F4} {Unit}";
    }
}