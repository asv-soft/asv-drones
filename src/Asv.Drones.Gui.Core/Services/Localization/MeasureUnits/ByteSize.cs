namespace Asv.Drones.Gui.Core
{
    public class ByteSize : IMeasureUnit<long>
    {
        private const long OneKb = 1024;
        private const long OneMb = OneKb * OneKb;
        private const long OneGb = OneMb * OneKb;

        public string GetUnit(long bytes)
        {
            //TODO: Localize
            return bytes switch
            {
                (< 0) => string.Empty,
                (< OneKb) => "B",
                (>= OneKb) and (< OneMb) => "KB", 
                (>= OneMb) and (< OneGb) => "MB",
                (>= OneGb)  => "GB",
            };
        }

        public string GetValue(long bytes)
        {
            return bytes switch
            {
                (< 0) => $"N/A", //TODO: Localize
                (< OneKb) => $"{bytes}",
                (>= OneKb) and (< OneMb) => $"{bytes / OneKb}",
                (>= OneMb) and (< OneGb) => $"{bytes / OneMb}",
                (>= OneGb)  => $"{bytes / OneMb}",
            };
        }
    }
}