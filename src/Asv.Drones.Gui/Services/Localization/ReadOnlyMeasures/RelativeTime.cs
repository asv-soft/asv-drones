using System;
using Asv.Drones.Gui.Api;

namespace Asv.Drones.Gui;

public class RelativeTime : IReadOnlyMeasureUnit<TimeSpan>
{
    public string? GetUnit(TimeSpan value)
    {
        return value.Ticks switch
        {
            (< TimeSpan.TicksPerSecond) => RS.RelativeTime_MillisecondsUnit,
            (>= TimeSpan.TicksPerSecond) and (<= TimeSpan.TicksPerMinute) => RS.RelativeTime_SecondsUnit,
            (>= TimeSpan.TicksPerMinute) => "",
        };
    }

    public string ConvertToString(TimeSpan value)
    {
        return value.Ticks switch
        {
            (< TimeSpan.TicksPerSecond) => $"{value.Milliseconds:000}",
            (>= TimeSpan.TicksPerSecond) and (< TimeSpan.TicksPerMinute) => $"{value.Seconds}",
            (>= TimeSpan.TicksPerMinute) and (< TimeSpan.TicksPerHour) => $"{value.Minutes:00}:{value.Seconds:00}",
            (>= TimeSpan.TicksPerHour) => $"{value.TotalHours:00}:{value.Minutes:00}:{value.Seconds:00}",
        };
    }
}