using System;
using R3;

namespace Asv.Drones;

public sealed class BusyFlag : IDisposable
{
    private readonly Subject<int> _delta = new();
    public Observable<bool> IsBusy { get; }

    public BusyFlag()
    {
        IsBusy = _delta
            .Scan(0, static (acc, d) => acc + d)
            .Select(static x => x > 0)
            .Prepend(false)
            .DistinctUntilChanged()
            .Publish()
            .RefCount();
    }

    public IDisposable Enter()
    {
        _delta.OnNext(+1);
        return Disposable.Create(() => _delta.OnNext(-1));
    }

    public void Dispose() => _delta.OnCompleted();
}
