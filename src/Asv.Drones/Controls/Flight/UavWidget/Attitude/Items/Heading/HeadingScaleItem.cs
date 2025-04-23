using System;

namespace Asv.Drones;

public class HeadingScaleItem : ScaleItem
{
    public HeadingScaleItem(
        double value,
        double valueRange,
        int index,
        int itemCount,
        double fullLength,
        double length
    )
        : base(value, valueRange, index, itemCount, fullLength, length, true) { }

    protected override string GetTitle(double value)
    {
        var v = value < 0 ? ((int)Math.Round(value) % 360) + 360 : (int)Math.Round(value) % 360;
        return v switch
        {
            0 => "N",
            45 => "NE",
            90 => "E",
            135 => "SE",
            180 => "S",
            225 => "SW",
            270 => "W",
            315 => "NW",
            360 => "N",
            _ => v.ToString("F0"),
        };
    }
}
