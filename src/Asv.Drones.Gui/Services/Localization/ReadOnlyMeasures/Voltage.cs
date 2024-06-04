using Asv.Drones.Gui.Api;

namespace Asv.Drones.Gui;

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