namespace Asv.Drones.Gui.Core
{
    public interface IMeasureUnit<in TValue>
    {
        string GetUnit(TValue value);
        string GetValue(TValue value);
    }

    public static class MeasureUnitExtensions
    {
        public static string GetValueWithUnits<TValue>(this IMeasureUnit<TValue> src, TValue value)
        {
            return src.GetValue(value) + src.GetUnit(value);
        }
    }
}