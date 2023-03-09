namespace Asv.Drones.Gui.Core;

public class Altitude : IMeasureUnit<double>
{
    private readonly AltitudeUnits _units;
    
    private const double Kilometer = 1000;
    private const double MetersInFeet = 0.3048;
    public Altitude(AltitudeUnits units)
    {
        _units = units;
    }

    public string GetUnit(double altitude)
    {
        return _units switch
        {
            AltitudeUnits.Meters => RS.Altitude_MeterUnit,
            AltitudeUnits.Feets => RS.Altitude_FeetUnit
        };
    }

    public string GetValue(double altitude)
    {
        return _units switch
        {
            AltitudeUnits.Meters => altitude >= Kilometer ? $"{altitude/Kilometer:F1}" : $"{altitude:F1}",
            AltitudeUnits.Feets => $"{altitude * MetersInFeet:F2}"
        };
    }
}