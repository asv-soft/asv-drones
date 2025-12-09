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
            0 => RS.HeadingScaleItem_Direction_N,
            45 => RS.HeadingScaleItem_Direction_NE,
            90 => RS.HeadingScaleItem_Direction_E,
            135 => RS.HeadingScaleItem_Direction_SE,
            180 => RS.HeadingScaleItem_Direction_S,
            225 => RS.HeadingScaleItem_Direction_SW,
            270 => RS.HeadingScaleItem_Direction_W,
            315 => RS.HeadingScaleItem_Direction_NW,
            360 => RS.HeadingScaleItem_Direction_N,
            _ => v.ToString("F0"),
        };
    }
}
