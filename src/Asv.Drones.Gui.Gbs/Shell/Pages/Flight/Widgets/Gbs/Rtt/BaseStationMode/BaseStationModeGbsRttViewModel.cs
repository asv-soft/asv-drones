using System.Reactive.Disposables;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink.V2.AsvGbs;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Gbs;

public class BaseStationModeGbsRttViewModel : GbsRttItem
{
    public BaseStationModeGbsRttViewModel()
    {
        BaseStationMode = "Idle";
    }

    public BaseStationModeGbsRttViewModel(IGbsDevice gbs)  : base(gbs, GenerateUri(gbs,"basestationmode"))
    {
        Order = 2;

        Gbs.DeviceClient.CustomMode
            .Subscribe(_ => BaseStationMode = _.ToString().Replace(nameof(AsvGbsCustomMode),""))
            .DisposeItWith(Disposable);
    }
    
    [Reactive]
    public string BaseStationMode { get; set; } = RS.GbsRttItem_ValueNotAvailable;
}