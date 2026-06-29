using Asv.Common;
using Asv.Modeling;
using R3;

namespace Asv.Drones.Api;

public class TelemetryViewModel<TData> : TextTileViewModel<TelemetryViewModel<TData>, TData>
{
    public const string BaseId = "telemetry";

    public TelemetryViewModel(
        string id,
        Observable<TData> dataSrc,
        Action<TelemetryViewModel<TData>, TData> action
    )
        : base($"{BaseId}.{id}", dataSrc, action)
    {
        Layout
            .Register(
                nameof(IsVisible),
                value => IsVisible = value,
                () => IsVisible,
                this.ObservePropertyChanged(x => x.IsVisible).Skip(1)
            )
            .DisposeItWith(Disposable);
        Layout.LoadWhenRootAttached(RootTracking).AddTo(ref DisposableBag);

        IconColor = TelemetryHelper.DefaultIconColor;
        TextColor = TelemetryHelper.DefaultTextColor;
        StatusColor = TelemetryHelper.DefaultStatusColor;
        StatusTextColor = TelemetryHelper.DefaultStatusTextColor;
    }
}
