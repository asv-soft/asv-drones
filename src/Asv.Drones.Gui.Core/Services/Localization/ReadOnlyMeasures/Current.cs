namespace Asv.Drones.Gui.Core;

public class Current : IReadOnlyMeasureUnit<double>
{
    public string GetUnit(double value)
    {
        return "A";
    }

    public string ConvertToString(double value)
    {
        return $"{value:F1}";
    }
}