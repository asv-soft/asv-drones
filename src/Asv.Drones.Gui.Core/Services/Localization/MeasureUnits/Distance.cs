namespace Asv.Drones.Gui.Core;

public class Distance : IMeasureUnit<double>
{
    private readonly DistanceUnits _units;

    private const double Kilometer = 1000;
    
    private const double MetersInInternationalNauticalMile = 1852;

    public Distance(DistanceUnits units)
    {
        _units = units;
    }

    public string GetUnit(double distance)
    {
        return _units switch
        {
            DistanceUnits.Meters => RS.Distance_MeterUnit,
            DistanceUnits.NauticalMiles => RS.Distance_InternationalNauticalMileUnit
        };
    }

    public string GetValue(double distance)
    {
        return _units switch
        {
            DistanceUnits.Meters => distance >= Kilometer ? $"{distance/Kilometer:F0}" : $"{distance:F0}",
            DistanceUnits.NauticalMiles => $"{distance/MetersInInternationalNauticalMile:F0}"
        };
    }

    public string GetValueSI(double distance)
    {
        return _units switch
        {
            DistanceUnits.Meters => $"{distance}",
            DistanceUnits.NauticalMiles => $"{distance * MetersInInternationalNauticalMile}"
        };
    }

}