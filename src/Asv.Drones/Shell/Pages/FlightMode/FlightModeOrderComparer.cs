using System.Collections.Generic;
using Asv.IO;

namespace Asv.Drones;

public class FlightModeOrderComparer : IComparer<IFlightModeOrderable>
{
    public static readonly FlightModeOrderComparer Instance = new();

    public int Compare(IFlightModeOrderable? x, IFlightModeOrderable? y)
    {
        if (x is null && y is null)
        {
            return 0;
        }

        if (x is null)
        {
            return -1;
        }

        if (y is null)
        {
            return 1;
        }

        var priorityCmp = x.DisplayPriority.CompareTo(y.DisplayPriority);
        if (priorityCmp != 0)
        {
            return priorityCmp;
        }

        var deviceCmp = CompareDeviceIds(x.DeviceId, y.DeviceId);
        if (deviceCmp != 0)
        {
            return deviceCmp;
        }

        return x.SubOrder.CompareTo(y.SubOrder);
    }

    private static int CompareDeviceIds(DeviceId? x, DeviceId? y)
    {
        if (x is null && y is null)
        {
            return 0;
        }

        if (x is null)
        {
            return 1; // без DeviceId — в конец группы
        }

        if (y is null)
        {
            return -1;
        }

        return x.CompareWithinProtocol(y);
    }
}
