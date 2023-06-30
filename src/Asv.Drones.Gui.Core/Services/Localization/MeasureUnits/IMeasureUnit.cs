#nullable enable
using Asv.Common;

namespace Asv.Drones.Gui.Core
{
   

    public interface IMeasureUnitItem<TValue, out TEnum>
    {
        public TEnum Id { get; }
        public string Title { get; }
        public string Unit { get; }
        public bool IsSiUnit { get; }
        public TValue ConvertFromSi(TValue siValue);
        public TValue ConvertToSi(TValue value);
        bool IsValid(string value);
        string? GetErrorMessage(string value);
        TValue ConvertToSi(string value);
        string FromSiToString(TValue value);
        string FromSiToStringWithUnits(TValue value);
    }
    
    public interface IMeasureUnit<TValue,TEnum>
    {
        string Title { get; }
        string Description { get; }
        IEnumerable<IMeasureUnitItem<TValue,TEnum>> AvailableUnits { get; }
        IRxEditableValue<IMeasureUnitItem<TValue,TEnum>> CurrentUnit { get; }
        IMeasureUnitItem<TValue,TEnum> SiUnit { get; }
    }
    
    public static class MeasureUnitExtensions
    {
        public static string FromSiToStringWithUnits<TValue, TEnum>(this IMeasureUnit<TValue, TEnum> src, TValue value)
        {
            return src.CurrentUnit.Value.FromSiToStringWithUnits(value);
        }
        
        public static string FromSiToString<TValue, TEnum>(this IMeasureUnit<TValue, TEnum> src, TValue value)
        {
            return src.CurrentUnit.Value.FromSiToString(value);
        }

        public static TValue ConvertFromSi<TValue,TEnum>(this IMeasureUnit<TValue,TEnum> src, TValue value)
       {
           return src.CurrentUnit.Value.ConvertFromSi(value);
       }
       public static TValue ConvertToSi<TValue,TEnum>(this IMeasureUnit<TValue,TEnum> src, TValue value)
       {
           return src.CurrentUnit.Value.ConvertToSi(value);
       }

       public static bool IsValid<TValue, TEnum>(this IMeasureUnit<TValue, TEnum> src, string value)
       {
           return src.CurrentUnit.Value.IsValid(value);
       }

       public static bool IsValid<TEnum>(this IMeasureUnit<double, TEnum> src, double minSiValue, double maxSiValue, string value)
       {
           if (src.CurrentUnit.Value.IsValid(value) == false) return false;
           if (src.CurrentUnit.Value.ConvertToSi(value) < minSiValue) return false;
           if (src.CurrentUnit.Value.ConvertToSi(value) > maxSiValue) return false;
           return true;
       }

       public static string? GetErrorMessage<TValue, TEnum>(this IMeasureUnit<TValue, TEnum> src, string value)
       {
           return src.CurrentUnit.Value.GetErrorMessage(value);
       }
       public static string? GetErrorMessage<TEnum>(this IMeasureUnit<double, TEnum> src,double minSiValue, double maxSiValue, string value)
       {
           var msg = src.CurrentUnit.Value.GetErrorMessage(value);
           if (msg.IsNullOrWhiteSpace() == false) return msg;
           var siValue = src.CurrentUnit.Value.ConvertToSi(value);
           if ( siValue< minSiValue) return $"Value must be greater than {src.CurrentUnit.Value.FromSiToStringWithUnits(minSiValue)} ({src.SiUnit.FromSiToStringWithUnits(siValue)})"; // TODO: Localize
           if ( siValue> maxSiValue) return $"Value must be less than {src.CurrentUnit.Value.FromSiToStringWithUnits(minSiValue)} {src.SiUnit.FromSiToStringWithUnits(siValue)}"; // TODO: Localize
           return null;
       }

       public static TValue ConvertToSi<TValue, TEnum>(this IMeasureUnit<TValue, TEnum> src, string value)
       {
           return src.CurrentUnit.Value.ConvertToSi(value);
       }
    }
}