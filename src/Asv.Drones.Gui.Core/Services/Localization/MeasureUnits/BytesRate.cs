namespace Asv.Drones.Gui.Core
{
    public class BytesRate : IMeasureUnit<double>
    {
        private const double OneKb = 1024.0;
        private const double OneMb = OneKb * OneKb;
        private const double OneGb = OneMb * OneKb;

        public string GetUnit(double bytesPerSec)
        {
            //TODO: Localize
            return bytesPerSec switch
            {
                (double.NaN) => string.Empty,
                (< 0) => string.Empty,
                (<= OneKb) => "B/s",
                (>= OneKb) and (< OneMb) => "KB/s",
                (>= OneMb) and (< OneGb) => "MB/s",
                (>= OneGb) => "GB/s",
            };
        }

        public string GetValue(double bytesPerSec)
        {
            return bytesPerSec switch
            {
                
                (double.NaN) => $"N/A", //TODO: Localize
                (<0) => $"N/A", //TODO: Localize
                (0) => $"{bytesPerSec,-4:F0}",
                (< 1) => $"{bytesPerSec,-4:F3}",
                (< OneKb) => $"{bytesPerSec,-4:F0}",
                (>= OneKb) and (< OneMb) => $"{bytesPerSec / OneKb,-4:F0}",
                (>= OneMb) and (< OneGb) => $"{bytesPerSec / OneKb,-4:F0}",
                (>= OneGb) => $"{bytesPerSec / OneMb,-4:F0}",
            };
        }

        
    }
}