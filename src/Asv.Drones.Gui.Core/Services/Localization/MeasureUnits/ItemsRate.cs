namespace Asv.Drones.Gui.Core
{
    public class ItemsRate : IMeasureUnit<double>
    {
        private const double OneKHz = 1000.0;
        private const double OneMHz = OneKHz * OneKHz;
        private const double OneGHz = OneMHz * OneKHz;

        public string GetUnit(double itemsPerSec)
        {
            //TODO: Localize
            return itemsPerSec switch
            {
                (double.NaN) => string.Empty,
                (< 0) => string.Empty,
                (<= OneKHz) => "Hz",
                (>= OneKHz) and (< OneMHz) => "KHz",
                (>= OneMHz) and (< OneGHz) => "MHz",
                (>= OneGHz) => "GHz",
            };
        }

        public string GetValue(double itemsPerSec)
        {
            return itemsPerSec switch
            {

                (double.NaN) => $"N/A", //TODO: Localize
                (< 0) => $"N/A", //TODO: Localize
                (0) => $"{itemsPerSec,-4:F0}",
                (< 1) => $"{itemsPerSec,-4:F3}",
                (< OneKHz) => $"{itemsPerSec,-4:F0}",
                (>= OneKHz) and (< OneMHz) => $"{itemsPerSec / OneKHz,-4:F0}",
                (>= OneMHz) and (< OneGHz) => $"{itemsPerSec / OneKHz,-4:F0}",
                (>= OneGHz) => $"{itemsPerSec / OneMHz,-4:F0}",
            };
        }
    }
}