namespace Asv.Drones.Gui.Api;

public interface IReadOnlyMeasureUnit<in TValue>
{
    string? GetUnit(TValue value);
    string ConvertToString(TValue value);
}

public static class ReadOnlyMeasureUnitExtensions
{
    public static string ConvertToStringWithUnits<TValue>(this IReadOnlyMeasureUnit<TValue> src, TValue value)
    {
        return src.ConvertToString(value) + src.GetUnit(value);
    }
}