namespace Asv.Drones.Gui.Core;

public class Velocity : IMeasureUnit<double>
{
    private readonly VelocityUnits _units;
    
    private const double MetersPerSecondInKilometersPerHour = 3.6;
    
    private const double MetersPerSecondInMilesPerHour =  2.236936;

    public Velocity(VelocityUnits units)
    {
        _units = units;
    }
    
    public string GetUnit(double value)
    {
        return _units switch
        {
            VelocityUnits.MetersPerSecond => RS.SettingsThemeViewModel_VelocityMetersPerSecondUnit,
            VelocityUnits.KilometersPerHour => RS.SettingsThemeViewModel_VelocityKilometersPerHourUnit,
            VelocityUnits.MilesPerHour => RS.SettingsThemeViewModel_VelocityMilesPerHourUnit
        };
    }

    public string GetValue(double value)
    {
        return _units switch
        {
            VelocityUnits.MetersPerSecond => $"{value:F1}",
            VelocityUnits.KilometersPerHour => $"{value * MetersPerSecondInKilometersPerHour:F1}",
            VelocityUnits.MilesPerHour => $"{value * MetersPerSecondInMilesPerHour:F1}"
        };
    }
}