using Asv.Common;

namespace Asv.Drones.Gui.Core
{
   

    public interface IMeasureUnitItem<TValue, out TEnum>
    {
        public TEnum Id { get; }
        public string Title { get; }
        public string Unit { get; }
        public bool IsSiUnit { get; }
        public TValue ConvertFromSI(TValue siValue);
        public TValue ConvertToSI(TValue value);
        bool IsValid(string value);
        string GetErrorMessage(string value);
        TValue ConvertToSI(string value);
        string FromSIToString(TValue value);
        string FromSIToStringWithUnits(TValue value);
    }
    
    public interface IMeasureUnit<TValue,TEnum>
    {
        string Title { get; }
        string Description { get; }
        IEnumerable<IMeasureUnitItem<TValue,TEnum>> AvailableUnits { get; }
        IRxEditableValue<IMeasureUnitItem<TValue,TEnum>> CurrentUnit { get; }
    }
    
    public static class MeasureUnitExtensions
    {
        public static string FromSIToStringWithUnits<TValue, TEnum>(this IMeasureUnit<TValue, TEnum> src, TValue value)
        {
            return src.CurrentUnit.Value.FromSIToStringWithUnits(value);
        }
        
        public static string FromSIToString<TValue, TEnum>(this IMeasureUnit<TValue, TEnum> src, TValue value)
        {
            return src.CurrentUnit.Value.FromSIToString(value);
        }

        public static TValue ConvertFromSi<TValue,TEnum>(this IMeasureUnit<TValue,TEnum> src, TValue value)
       {
           return src.CurrentUnit.Value.ConvertFromSI(value);
       }
       public static TValue ConvertToSI<TValue,TEnum>(this IMeasureUnit<TValue,TEnum> src, TValue value)
       {
           return src.CurrentUnit.Value.ConvertToSI(value);
       }

       public static bool IsValid<TValue, TEnum>(this IMeasureUnit<TValue, TEnum> src, string value)
       {
           return src.CurrentUnit.Value.IsValid(value);
       }

       public static string GetErrorMessage<TValue, TEnum>(this IMeasureUnit<TValue, TEnum> src, string value)
       {
           return src.CurrentUnit.Value.GetErrorMessage(value);
       }

       public static TValue ConvertToSI<TValue, TEnum>(this IMeasureUnit<TValue, TEnum> src, string value)
       {
           return src.CurrentUnit.Value.ConvertToSI(value);
       }
    }
}