using Asv.Avalonia;
using Asv.Common;
using R3;

namespace Asv.Drones.Api;

public class TextTileViewModel<TSelf, TData> : TextTileViewModel
    where TSelf : TextTileViewModel<TSelf, TData>
{
    public TextTileViewModel(string id, Observable<TData> dataSrc, Action<TSelf, TData> action)
        : base(id)
    {
        dataSrc
            .CombineLatest(
                this.ObservePropertyChanged(x => x.IsVisible).Prepend(IsVisible),
                (data, isVisible) => (Data: data, IsVisible: isVisible)
            )
            .Where(x => x.IsVisible)
            .Subscribe(x => action((TSelf)this, x.Data))
            .DisposeItWith(Disposable);
    }
}
