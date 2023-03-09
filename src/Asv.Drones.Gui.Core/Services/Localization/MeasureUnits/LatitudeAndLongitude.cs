namespace Asv.Drones.Gui.Core;

public class LatitudeAndLongitude : IMeasureUnit<double>
{
    private readonly LatitudeLongitudeUnits _units;

    private double _minutes;
    
    public LatitudeAndLongitude(LatitudeLongitudeUnits units)
    {
        _units = units;
    }

    public string GetUnit(double value)
    {
        return _units switch
        {
            LatitudeLongitudeUnits.Degrees => "°",
            LatitudeLongitudeUnits.DegreesMinutesSeconds => ""
        };
    }

    public string GetValue(double value)
    {
        return _units switch
        {
            LatitudeLongitudeUnits.Degrees => $"{value:F5}",
            LatitudeLongitudeUnits.DegreesMinutesSeconds => $"{value:F0}°{_minutes = (value - (int)value) * 60:F0}'{(_minutes - (int)_minutes) * 60:F2}''"
        };
    }

    public string GetValueSI(double value) => $"{value}";
}