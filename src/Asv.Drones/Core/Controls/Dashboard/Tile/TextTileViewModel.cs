using Asv.Avalonia;
using Asv.Common;
using R3;

namespace Asv.Drones;

public class TextTileViewModel<TSelf, TData> : TextTileViewModel
    where TSelf : TextTileViewModel<TSelf, TData>
{
    public TextTileViewModel(string id, Observable<TData> dataSrc, Action<TSelf, TData> action)
        : base(id)
    {
        dataSrc
            .Subscribe(d =>
            {
                if (!IsVisible)
                {
                    return;
                }

                action((TSelf)this, d);
            })
            .DisposeItWith(Disposable);
    }
}
