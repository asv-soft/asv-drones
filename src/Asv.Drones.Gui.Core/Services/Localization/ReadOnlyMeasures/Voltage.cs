namespace Asv.Drones.Gui.Core;

public class Voltage : IReadOnlyMeasureUnit<double>
{
    public string? GetUnit(double value)
    {
        return RS.Voltage_Unit;
    }

    public string ConvertToString(double value)
    {
        return $"{value:F1}";
    }
}