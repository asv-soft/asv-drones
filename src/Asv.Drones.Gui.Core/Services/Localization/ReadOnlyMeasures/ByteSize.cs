namespace Asv.Drones.Gui.Core
{
    public class ByteSize : IReadOnlyMeasureUnit<long>
    {
        private const long OneKb = 1024;
        private const long OneMb = OneKb * OneKb;
        private const long OneGb = OneMb * OneKb;

        public string? GetUnit(long bytes)
        {
            return bytes switch
            {
                (< 0) => string.Empty,
                (< OneKb) => RS.ByteSize_BytesUnit,
                (>= OneKb) and (< OneMb) => RS.ByteSize_KilobytesUnit, 
                (>= OneMb) and (< OneGb) => RS.ByteSize_MegabytesUnit,
                (>= OneGb)  => RS.ByteSize_GigabytesUnit,
            };
        }

        public string ConvertToString(long bytes)
        {
            return bytes switch
            {
                (< 0) => RS.Common_NotAvailable,
                (< OneKb) => $"{bytes}",
                (>= OneKb) and (< OneMb) => $"{bytes / OneKb}",
                (>= OneMb) and (< OneGb) => $"{bytes / OneMb}",
                (>= OneGb)  => $"{bytes / OneMb}",
            };
        }
    }
}