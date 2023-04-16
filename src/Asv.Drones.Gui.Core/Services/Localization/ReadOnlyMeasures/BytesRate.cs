namespace Asv.Drones.Gui.Core
{
    public class BytesRate : IReadOnlyMeasureUnit<double>
    {
        private const double OneKb = 1024.0;
        private const double OneMb = OneKb * OneKb;
        private const double OneGb = OneMb * OneKb;

        public string? GetUnit(double bytesPerSec)
        {
            return bytesPerSec switch
            {
                (double.NaN) => string.Empty,
                (< 0) => string.Empty,
                (<= OneKb) => RS.BytesRate_BytesPerSecondUnit,
                (>= OneKb) and (< OneMb) => RS.BytesRate_KiloBytesPerSecondUnit,
                (>= OneMb) and (< OneGb) => RS.BytesRate_MegaBytesPerSecondUnit,
                (>= OneGb) => RS.BytesRate_GigaBytesPerSecondUnit,
            };
        }

        public string ConvertToString(double bytesPerSec)
        {
            return bytesPerSec switch
            {
                (double.NaN) => RS.Common_NotAvailable,
                (<0) => RS.Common_NotAvailable,
                (0) => $"{bytesPerSec,-4:F0}",
                (< 1) => $"{bytesPerSec,-4:F3}",
                (< OneKb) => $"{bytesPerSec,-4:F0}",
                (>= OneKb) and (< OneMb) => $"{bytesPerSec / OneKb,-4:F0}",
                (>= OneMb) and (< OneGb) => $"{bytesPerSec / OneKb,-4:F0}",
                (>= OneGb) => $"{bytesPerSec / OneMb,-4:F0}",
            };
        }

        public string GetValueSI(double bytesPerSec) => ConvertToString(bytesPerSec);
    }
}