namespace Asv.Drones.Gui.Core;

/// <summary>
/// Represents an interface for read-only measure units.
/// </summary>
/// <typeparam name="TValue">The type of value that the measure units can be applied to.</typeparam>
public interface IReadOnlyMeasureUnit<in TValue>
{
    /// <summary>
    /// Retrieves the unit associated with the given value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="value">The value for which to retrieve the unit.</param>
    /// <returns>The unit associated with the given value, or null if no unit is found.</returns>
    string? GetUnit(TValue value);

    /// <summary>
    /// Converts the given value to a string representation.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <returns>The string representation of the value.</returns>
    string ConvertToString(TValue value);

}

/// <summary>
/// Extension methods for the readonly measure unit.
/// </summary>
public static class ReadOnlyMeasureUnitExtensions
{
    /// <summary>
    /// Converts the value to a string representation with units using the provided measure unit.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="src">The measure unit used for conversion.</param>
    /// <param name="value">The value to be converted.</param>
    /// <returns>The string representation of the value with units.</returns>
    public static string ConvertToStringWithUnits<TValue>(this IReadOnlyMeasureUnit<TValue> src, TValue value)
    {
        return src.ConvertToString(value) + src.GetUnit(value);
    }
        
}