using System.Reactive.Linq;
using Asv.Common;
using Asv.Mavlink.Vehicle;

namespace Asv.Drones.Gui.Core;

public class Ruler : DisposableViewModelBase
{
    public IRxEditableValue<GeoPoint?> Start { get; }
    public IRxEditableValue<GeoPoint?> Stop { get; }
    public IRxEditableValue<double> Distance { get; }
    public IRxEditableValue<bool?> IsVisible { get; }

    public Ruler()
    {
        Start = new RxValue<GeoPoint?>(new GeoPoint()).DisposeItWith(Disposable);
        Stop = new RxValue<GeoPoint?>(new GeoPoint()).DisposeItWith(Disposable);
        Distance = new RxValue<double>().DisposeItWith(Disposable);
        IsVisible = new RxValue<bool?>().DisposeItWith(Disposable);

        Start.Merge(Stop).Subscribe(_ => CalculateDistance()).DisposeItWith(Disposable);
    }

    private void CalculateDistance()
    {
        Distance.OnNext(GeoMath.Distance(Start.Value, Stop.Value));
    }
}