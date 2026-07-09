namespace Asv.Drones.Api;

public class FlightWidgetSectionsComparer : IComparer<IFlightWidgetSection>
{
    public static readonly FlightWidgetSectionsComparer Instance = new();

    public int Compare(IFlightWidgetSection? x, IFlightWidgetSection? y)
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

        return x.Order.CompareTo(y.Order);
    }
}
