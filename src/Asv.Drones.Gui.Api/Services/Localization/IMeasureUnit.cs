#nullable enable
using System.Globalization;
using Asv.Common;
using Avalonia;
using Avalonia.Data.Converters;

namespace Asv.Drones.Gui.Api
{
    public interface IMeasureUnitItem<TValue>
    {
        public string Title { get; }
        public string Unit { get; }
        public bool IsSiUnit { get; }
        public TValue ConvertFromSi(TValue siValue);
        public TValue ConvertToSi(TValue value);
        public TValue Parse(string? value);
        bool IsValid(string? value);
        string GetErrorMessage(string? value);
        string Print(TValue value);
        string PrintWithUnits(TValue value);

        public TValue ConvertToSi(string? value)
        {
            return ConvertToSi(Parse(value));
        }

        public string FromSiToString(TValue value)
        {
            return Print(ConvertFromSi(value));
        }

        public string FromSiToStringWithUnits(TValue value)
        {
            return PrintWithUnits(ConvertFromSi(value));
        }
    }

    public interface IMeasureUnitItem<TValue, out TEnum> : IMeasureUnitItem<TValue>
    {
        public TEnum Id { get; }
    }

    public interface IMeasureUnit<TValue, TEnum>
    {
        string Title { get; }
        string Description { get; }
        IEnumerable<IMeasureUnitItem<TValue, TEnum>> AvailableUnits { get; }
        IRxEditableValue<IMeasureUnitItem<TValue, TEnum>> CurrentUnit { get; }
        IMeasureUnitItem<TValue, TEnum> SiUnit { get; }

        public string FromSiToStringWithUnits(TValue value)
        {
            return CurrentUnit.Value.FromSiToStringWithUnits(value);
        }

        public string FromSiToString(TValue value)
        {
            return CurrentUnit.Value.FromSiToString(value);
        }

        public TValue ConvertFromSi(TValue value)
        {
            return CurrentUnit.Value.ConvertFromSi(value);
        }

        public TValue ConvertToSi(TValue value)
        {
            return CurrentUnit.Value.ConvertToSi(value);
        }

        public TValue ConvertToSi(string? value)
        {
            return CurrentUnit.Value.ConvertToSi(value);
        }

        public bool IsValid(string? value)
        {
            return CurrentUnit.Value.IsValid(value);
        }
    }

    public static class MeasureUnitExtensions
    {
        private const string DefaultErrorMessage = "Something went wrong";
        
        public static bool IsValid<TEnum>(this IMeasureUnit<double, TEnum> src, double minSiValue, double maxSiValue,
            string value)
        {
            if (src.CurrentUnit.Value.IsValid(value) == false) return false;
            if (src.CurrentUnit.Value.ConvertToSi(value) < minSiValue) return false;
            if (src.CurrentUnit.Value.ConvertToSi(value) > maxSiValue) return false;
            return true;
        }

        public static string GetErrorMessage<TValue, TEnum>(this IMeasureUnit<TValue, TEnum> src, string? value)
        {
            return src.CurrentUnit.Value.GetErrorMessage(value);
        }

        public static string GetErrorMessage<TEnum>(this IMeasureUnit<double, TEnum> src, double minSiValue,
            double maxSiValue, string? value)
        {
            var msg = src.CurrentUnit.Value.GetErrorMessage(value);
            if (string.IsNullOrWhiteSpace(msg) == false) return msg;
            var siValue = src.CurrentUnit.Value.ConvertToSi(value);
            if (siValue < minSiValue)
                return string.Format(RS.MeasureUnitExtensions_ErrorMessage_GreaterValue,
                    src.CurrentUnit.Value.FromSiToStringWithUnits(minSiValue),
                    src.SiUnit.FromSiToStringWithUnits(siValue));
            if (siValue > maxSiValue)
                return string.Format(RS.MeasureUnitExtensions_ErrorMessage_LesserValue,
                    src.CurrentUnit.Value.FromSiToStringWithUnits(minSiValue),
                    src.SiUnit.FromSiToStringWithUnits(siValue));
            return DefaultErrorMessage;
        }
    }


    public static class MeasureUnitConverter
    {
        static MeasureUnitConverter()
        {
            DoubleInstance = new MeasureUnitConverter<double>();
            UlongInstance = new MeasureUnitConverter<ulong>();
        }

        public static MeasureUnitConverter<ulong> UlongInstance { get; set; }
        public static MeasureUnitConverter<double> DoubleInstance { get; }
    }


    public class MeasureUnitConverter<TValue> : IMultiValueConverter
    {
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