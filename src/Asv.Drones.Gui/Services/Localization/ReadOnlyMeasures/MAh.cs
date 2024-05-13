using Asv.Drones.Gui.Api;

namespace Asv.Drones.Gui;

public class MAh : IReadOnlyMeasureUnit<double>
{
    public string? GetUnit(double value)
    {
        return "mAh";
    }

    public string ConvertToString(double value)
    {
        return $"{value:F1}";
    }
}