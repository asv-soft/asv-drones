namespace Asv.Drones.Gui.Core
{
    public interface IMeasureUnit<in TValue>
    {
        string GetUnit(TValue value);
        string GetValue(TValue value);
        string GetValueSI(TValue value);
    }

    public static class MeasureUnitExtensions
    {
        public static string GetValueWithUnits<TValue>(this IMeasureUnit<TValue> src, TValue value)
        {
            return src.GetValue(value) + src.GetUnit(value);
        }
        
        public static double GetDoubleValue(this IMeasureUnit<double> src, double value, bool valueInSi)
        {
            if (valueInSi)
            {
                return double.Parse(src.GetValueSI(value));
            }
            return double.Parse(src.GetValue(value));
        }
        
        public static double GetDoubleValue(this IMeasureUnit<double> src, string value, bool valueInSi)
        {
            double tempValue = 0;
            if (double.TryParse(value, out tempValue))
            {
                if (valueInSi)
                {
                    return double.Parse(src.GetValueSI(tempValue));
                }
                return double.Parse(src.GetValue(tempValue));    
            }
            return tempValue;
        }
    }
}