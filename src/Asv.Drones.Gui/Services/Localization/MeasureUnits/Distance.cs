using System.Globalization;
using Asv.Cfg;
using Asv.Drones.Gui.Api;
using Avalonia.Controls.Documents;

namespace Asv.Drones.Gui;

public class Distance : MeasureUnitBase<double, DistanceUnits>
{
    private const double MetersInInternationalNauticalMile = 1852;

    private static readonly IMeasureUnitItem<double, DistanceUnits>[] _units =
    {
        new DoubleMeasureUnitItem<DistanceUnits>(DistanceUnits.Meters, RS.Distance_Meters_Title,
            RS.Distance_Meters_Unit, true, "F0", 1),
        new DoubleMeasureUnitItem<DistanceUnits>(DistanceUnits.NauticalMiles, RS.Distance_NauticalMiles_Title,
            RS.Distance_NauticalMiles_Unit, false, "F4", MetersInInternationalNauticalMile),
    };

    public Distance(IConfiguration cfgSvc, string cfgKey) : base(cfgSvc, cfgKey, _units)
    {
    }

    public override string Title => RS.Distance_Title;
    public override string Description => RS.Distance_Description;


}

public class DistanceMeters : IMeasureUnitItem<double, DistanceUnits>
{
    public DistanceUnits Id { get; }
    public string Title { get; } = RS.Distance_Meters_Title;
    public string Unit { get; } = RS.Distance_Meters_Unit;
    public bool IsSiUnit { get; } = false;
    
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
    
    //Not usable
    public string? GetErrorMessage(string? value)
    {
        return value;
    }
    //Not usable
    public string Print(double value)
    {
        return $"{value}";
    }
    //Not usable
    public string PrintWithUnits(double value)
    {
        return $"{value} {Unit}";
    }

}