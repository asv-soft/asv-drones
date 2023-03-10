namespace Asv.Drones.Gui.Core
{
    public class ItemsRate : IReadOnlyMeasureUnit<double>
    {
        private const double OneKHz = 1000.0;
        private const double OneMHz = OneKHz * OneKHz;
        private const double OneGHz = OneMHz * OneKHz;

        public string GetUnit(double itemsPerSec)
        {
            return itemsPerSec switch
            {
                (double.NaN) => string.Empty,
                (< 0) => string.Empty,
                (<= OneKHz) => RS.ItemsRate_HerzUnit,
                (>= OneKHz) and (< OneMHz) => RS.ItemsRate_KiloHerzUnit,
                (>= OneMHz) and (< OneGHz) => RS.ItemsRate_MegaHerzUnit,
                (>= OneGHz) => RS.ItemsRate_GigaHerzUnit,
            };
        }

        public string ConvertToString(double itemsPerSec)
        {
            return itemsPerSec switch
            {
                (double.NaN) => RS.Common_NotAvailable,
                (< 0) => RS.Common_NotAvailable,
                (0) => $"{itemsPerSec,-4:F0}",
                (< 1) => $"{itemsPerSec,-4:F1}",
                (< OneKHz) => $"{itemsPerSec,-4:F0}",
                (>= OneKHz) and (< OneMHz) => $"{itemsPerSec / OneKHz,-4:F0}",
                (>= OneMHz) and (< OneGHz) => $"{itemsPerSec / OneKHz,-4:F0}",
                (>= OneGHz) => $"{itemsPerSec / OneMHz,-4:F0}",
            };
        }
        

    }
}