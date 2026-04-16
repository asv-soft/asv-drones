namespace Asv.Drones.Api;

public class FlightWidgetsComparer : IComparer<IFlightWidget>
{
    public static readonly FlightWidgetsComparer Instance = new();

    public int Compare(IFlightWidget? x, IFlightWidget? y)
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

        var typeComparison = CompareWidgetGroups(x, y);
        if (typeComparison != 0)
        {
            return typeComparison;
        }

        return x.Order.CompareTo(y.Order);
    }

    private static int CompareWidgetGroups(IFlightWidget x, IFlightWidget y)
    {
        return StringComparer.Ordinal.Compare(GetGroupKey(x), GetGroupKey(y));
    }

    private static string GetGroupKey(IFlightWidget widget)
    {
        return widget.GetType().FullName ?? string.Empty;
    }
}
