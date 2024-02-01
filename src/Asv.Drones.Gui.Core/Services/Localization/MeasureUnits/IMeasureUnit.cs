#nullable enable
using System.Globalization;
using Asv.Common;
using Avalonia;
using Avalonia.Data.Converters;

namespace Asv.Drones.Gui.Core
{
    /// <summary>
    /// Represents a measure unit item.
    /// </summary>
    /// <typeparam name="TValue">The type of value for the measure unit item.</typeparam>
    public interface IMeasureUnitItem<TValue>
    {
        /// <summary>
        /// Gets the title of the property.
        /// </summary>
        /// <remarks>
        /// The title is a read-only property that represents the title of the object.
        /// </remarks>
        /// <value>
        /// A string value representing the title of the object.
        /// </value>
        public string Title { get; }

        /// <summary>
        /// Gets the unit of the property.
        /// </summary>
        /// <returns>The unit of the property.</returns>
        /// <remarks>
        /// This property represents the unit associated with a specific property. The unit can be used to indicate the measurement type
        /// of the property's value. For example, if the property represents a length, the unit can be "m" for meters or "ft" for feet.
        /// </remarks>
        public string Unit { get; }

        /// <summary>
        /// Gets a value indicating whether the property represents an SI unit.
        /// </summary>
        /// <value><c>true</c> if the property is an SI unit; otherwise, <c>false</c>.</value>
        public bool IsSiUnit { get; }
        public TValue ConvertFromSi(TValue siValue);

        /// <summary>
        /// Converts the specified value to SI units.
        /// </summary>
        /// <typeparam name="TValue">The type of the value to convert.</typeparam>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value in SI units.</returns>
        public TValue ConvertToSi(TValue value);

        /// <summary>
        /// Parses the specified string value and converts it to a value of type TValue.
        /// </summary>
        /// <param name="value">The string value to parse.</param>
        /// <returns>The parsed value of type TValue.</returns>
        /// <remarks>
        /// This method converts the string value to the specified type using the default parsing mechanism of the type.
        /// If the value is null, it returns the default value of type TValue.
        /// </remarks>
        /// <typeparam name="TValue">The type to which the value should be converted.</typeparam>
        /// <returns>The parsed value of type TValue.</returns>
        public TValue Parse(string? value);

        /// <summary>
        /// Checks if the given value is valid.
        /// </summary>
        /// <param name="value">The value to be checked.</param>
        /// <returns>True if the value is valid, otherwise false.</returns>
        bool IsValid(string? value);

        /// <summary>
        /// Retrieves the error message associated with the specified value.
        /// </summary>
        /// <param name="value">The value to retrieve the error message for.</param>
        /// <returns>The error message associated with the specified value.</returns>
        /// <remarks>
        /// If the value is null, the method returns null as well.
        /// </remarks>
        string? GetErrorMessage(string? value);

        /// <summary>
        /// Prints the value of the given type.
        /// </summary>
        /// <typeparam name="TValue">The type of value.</typeparam>
        /// <param name="value">The value to print.</param>
        /// <returns>The printed value as a string.</returns>
        string Print(TValue value);

        /// <summary>
        /// Prints the given value with units.
        /// </summary>
        /// <param name="value">The value to be printed.</param>
        /// <returns>The formatted value with units as a string.</returns>
        string PrintWithUnits(TValue value);

        /// <summary>
        /// Converts the given value to the SI unit representation.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The value converted to the SI unit representation.</returns>
        public TValue ConvertToSi(string? value)
        {
            return ConvertToSi(Parse(value));
        }

        /// <summary>
        /// Converts the specified value from SI units to string representation.
        /// </summary>
        /// <typeparam name="TValue">The type of the value to convert.</typeparam>
        /// <param name="value">The value to convert from SI units.</param>
        /// <returns>The string representation of the converted value.</returns>
        public string FromSiToString(TValue value)
        {
            return Print(ConvertFromSi(value));
        }

        /// <summary>
        /// Converts the given value from SI units to a formatted string with units.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>A string representation of the converted value with units.</returns>
        public string FromSiToStringWithUnits(TValue value)
        {
            return PrintWithUnits(ConvertFromSi(value));
        }
    }

    /// <summary>
    /// Represents a measure unit item with a specific identifier.
    /// </summary>
    /// <typeparam name="TValue">The type of the value associated with the measure unit item.</typeparam>
    /// <typeparam name="TEnum">The type of the enum identifier associated with the measure unit item.</typeparam>
    public interface IMeasureUnitItem<TValue, out TEnum>:IMeasureUnitItem<TValue>
    {
        /// <summary>
        /// Gets the value of the property Id.
        /// </summary>
        /// <typeparam name="TEnum">The enum type of the Id.</typeparam>
        /// <returns>The value of the property Id.</returns>
        /// <example>
        /// Example usage:
        /// <code>
        /// Console.WriteLine(property.Id);
        /// </code>
        /// </example>
        public TEnum Id { get; }
        
    }

    /// <summary>
    /// Interface for representing a measure unit.
    /// </summary>
    /// <typeparam name="TValue">The type of the value associated with the unit.</typeparam>
    /// <typeparam name="TEnum">The enum type representing the available units.</typeparam>
    public interface IMeasureUnit<TValue,TEnum>
    {
        /// <summary>
        /// Gets the title of the property.
        /// </summary>
        /// <returns>
        /// The title of the property.
        /// </returns>
        string Title { get; }

        /// <summary>
        /// Gets the description of the property.
        /// </summary>
        /// <returns>
        /// A string representing the description of the property.
        /// </returns>
        string Description { get; }

        /// <summary>
        /// Gets the collection of available units for the property.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> of <see cref="IMeasureUnitItem{TValue, TEnum}"/> representing the available units.
        /// </returns>
        /// <typeparam name="TValue">The type of value associated with the units.</typeparam>
        /// <typeparam name="TEnum">The type of enum used to represent the units.</typeparam>
        IEnumerable<IMeasureUnitItem<TValue,TEnum>> AvailableUnits { get; }

        /// <summary>
        /// Gets the current unit for the editable value.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <typeparam name="TEnum">The enumeration type representing the units.</typeparam>
        /// <returns>The current unit for the editable value.</returns>
        IRxEditableValue<IMeasureUnitItem<TValue,TEnum>> CurrentUnit { get; }

        /// Gets or sets the SI unit for the property.
        /// @remarks
        /// The SI unit determines the standard unit of measurement for the property value.
        /// @typeParam TValue - The type of the property value.
        /// @typeParam TEnum - The enum type representing the available measurement units.
        /// @returns The SI unit for the property.
        /// /
        IMeasureUnitItem<TValue,TEnum> SiUnit { get; }

        /// <summary>
        /// Converts the given value from SI units to a string representation with units.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>The string representation of the converted value with units.</returns>
        public string FromSiToStringWithUnits(TValue value)
        {
            return CurrentUnit.Value.FromSiToStringWithUnits(value);
        }

        /// <summary>
        /// Converts the value from the SI unit to a string representation.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="value">The value to convert.</param>
        /// <returns>A string representation of the converted value.</returns>
        public string FromSiToString(TValue value)
        {
            return CurrentUnit.Value.FromSiToString(value);
        }

        /// <summary>
        /// Converts a value from the SI unit of the current unit to the unit's native value.
        /// </summary>
        /// <typeparam name="TValue">The type of the value to convert.</typeparam>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        public TValue ConvertFromSi(TValue value)
        {
            return CurrentUnit.Value.ConvertFromSi(value);
        }

        /// <summary>
        /// Converts the given value to the SI unit.
        /// </summary>
        /// <param name="value">The value to be converted.</param>
        /// <returns>The converted value in SI unit.</returns>
        /// <typeparam name="TValue">The type of the value to be converted.</typeparam>
        public TValue ConvertToSi(TValue value)
        {
            return CurrentUnit.Value.ConvertToSi(value);
        }

        /// <summary>
        /// Converts the given value to the equivalent value in the International System of Units (SI).
        /// </summary>
        /// <param name="value">The value to be converted as a string.</param>
        /// <returns>The converted value of type TValue.</returns>
        public TValue ConvertToSi(string? value)
        {
            return CurrentUnit.Value.ConvertToSi(value);
        }

        /// <summary>
        /// Checks if the given value is valid.
        /// </summary>
        /// <param name="value">The value to be checked.</param>
        /// <returns>True if the value is valid, false otherwise.</returns>
        public bool IsValid(string? value)
        {
            return CurrentUnit.Value.IsValid(value);
        }
    }
    
    public static class MeasureUnitExtensions
    {
        /// <summary>
        /// Checks if the provided value is valid within the given range.
        /// </summary>
        /// <typeparam name="TEnum">The enum type representing the measure unit.</typeparam>
        /// <param name="src">The instance of an object implementing the IMeasureUnit interface.</param>
        /// <param name="minSiValue">The minimum valid value in the SI unit.</param>
        /// <param name="maxSiValue">The maximum valid value in the SI unit.</param>
        /// <param name="value">The value to be checked.</param>
        /// <returns>
        /// True if the value is valid within the provided range; otherwise, false.
        /// </returns>
        public static bool IsValid<TEnum>(this IMeasureUnit<double, TEnum> src, double minSiValue, double maxSiValue, string value)
       {
           if (src.CurrentUnit.Value.IsValid(value) == false) return false;
           if (src.CurrentUnit.Value.ConvertToSi(value) < minSiValue) return false;
           if (src.CurrentUnit.Value.ConvertToSi(value) > maxSiValue) return false;
           return true;
       }

        /// <summary>
        /// Retrieves the error message for the given value.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <param name="src">The <see cref="IMeasureUnit{TValue, TEnum}"/> object.</param>
        /// <param name="value">The input value.</param>
        /// <returns>The error message for the given value.</returns>
        public static string? GetErrorMessage<TValue, TEnum>(this IMeasureUnit<TValue, TEnum> src, string? value)
       {
           return src.CurrentUnit.Value.GetErrorMessage(value);
       }

        /// <summary>
        /// Get the error message based on the provided values.
        /// </summary>
        /// <typeparam name="TEnum">The enum type representing the measure unit.</typeparam>
        /// <param name="src">The source IMeasureUnit instance.</param>
        /// <param name="minSiValue">The minimum SI value.</param>
        /// <param name="maxSiValue">The maximum SI value.</param>
        /// <param name="value">The value to check against the minimum and maximum SI values.</param>
        /// <returns>The error message if value is out of range, or null if value is within the range.</returns>
        public static string? GetErrorMessage<TEnum>(this IMeasureUnit<double, TEnum> src,double minSiValue, double maxSiValue, string? value)
       {
           var msg = src.CurrentUnit.Value.GetErrorMessage(value);
           if (string.IsNullOrWhiteSpace(msg) == false) return msg;
           var siValue = src.CurrentUnit.Value.ConvertToSi(value);
           if ( siValue< minSiValue) return string.Format(RS.MeasureUnitExtensions_ErrorMessage_GreaterValue, src.CurrentUnit.Value.FromSiToStringWithUnits(minSiValue), src.SiUnit.FromSiToStringWithUnits(siValue));
           if (siValue > maxSiValue) return string.Format(RS.MeasureUnitExtensions_ErrorMessage_LesserValue, src.CurrentUnit.Value.FromSiToStringWithUnits(minSiValue), src.SiUnit.FromSiToStringWithUnits(siValue));
           return null;
       }

    }


    /// <summary>
    /// Represents a class that provides conversion between different units of measure.
    /// </summary>
    public static class MeasureUnitConverter
    {
        /// <summary>
        /// This class represents a MeasureUnitConverter that can convert between different measure units.
        /// </summary>
        static MeasureUnitConverter()
        {
            DoubleInstance = new MeasureUnitConverter<double>();
            UlongInstance = new MeasureUnitConverter<ulong>();
        }

        /// <summary>
        /// Gets or sets the instance of <see cref="MeasureUnitConverter"/> for <see cref="ulong"/> values.
        /// </summary>
        /// <value>
        /// The instance of <see cref="MeasureUnitConverter"/> for <see cref="ulong"/> values.
        /// </value>
        public static MeasureUnitConverter<ulong> UlongInstance { get; set; }

        /// <summary>
        /// Gets the instance of MeasureUnitConverter<double> class.
        /// </summary>
        /// <value>
        /// The instance of MeasureUnitConverter<double> class.
        /// </value>
        public static MeasureUnitConverter<double> DoubleInstance { get; }
    }


    /// <summary>
    /// A class that represents a measure unit converter.
    /// </summary>
    /// <typeparam name="TValue">The type of value to convert.</typeparam>
    public class MeasureUnitConverter<TValue> : IMultiValueConverter
    {
        /// <summary>
        /// Converts a list of values to the specified target type using the specified culture information.
        /// </summary>
        /// <param name="values">The list of values to convert.</param>
        /// <param name="targetType">The target type to convert the values to.</param>
        /// <param name="parameter">An optional parameter to use during the conversion process.</param>
        /// <param name="culture">The culture information to use for the conversion.</param>
        /// <returns>The converted value as an <see cref="object"/>. If the conversion fails, <see cref="AvaloniaProperty.UnsetValue"/> is returned.</returns>
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Count == 1)
                return System.Convert.ChangeType(values[0], targetType, culture);
            if (values is [_, IMeasureUnitItem<TValue> measureUnit, ..])
            {
                var value = (TValue)System.Convert.ChangeType(values[0], typeof(TValue), culture)!;
                return measureUnit.Print(value);
            }

            return AvaloniaProperty.UnsetValue;
        }
    }
}