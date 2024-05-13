namespace Asv.Drones.Gui.Api;

public class PropertyComparer<T, TField> : IComparer<T>
    where TField : IComparable<TField>
{
    private readonly Func<T, TField> _callback;

    public PropertyComparer(Func<T, TField> callback)
    {
        _callback = callback;
    }

    public int Compare(T? x, T? y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (ReferenceEquals(null, y)) return 1;
        if (ReferenceEquals(null, x)) return -1;
        return _callback(x).CompareTo(_callback(y));
    }
}