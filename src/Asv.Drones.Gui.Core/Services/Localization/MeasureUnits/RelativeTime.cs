namespace Asv.Drones.Gui.Core;

public class RelativeTime : IMeasureUnit<TimeSpan>
{
    public string GetUnit(TimeSpan value)
    {
        return value.Ticks switch
        {
            (< TimeSpan.TicksPerSecond) => RS.RelativeTime_MillisecondsUnit,
            (>= TimeSpan.TicksPerSecond) and (<= TimeSpan.TicksPerMinute) => RS.RelativeTime_SecondsUnit,
            (>= TimeSpan.TicksPerMinute) => "",
        };
    }

    public string GetValue(TimeSpan value)
    {
        return value.Ticks switch
        {
            (< TimeSpan.TicksPerSecond) => $"{value.Milliseconds}",
            (>= TimeSpan.TicksPerSecond) and (< TimeSpan.TicksPerMinute) => $"{value.Seconds}",
            (>= TimeSpan.TicksPerMinute) => $"{value.TotalHours}:{value.Minutes}:{value.Seconds}",
        };
    }
    
    public string GetValueSI(TimeSpan value) => $"{value}";

}