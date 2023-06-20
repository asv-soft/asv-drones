using System.Reactive.Disposables;
using System.Reactive.Linq;
using Asv.Common;
using Asv.Drones.Gui.Core;
using Asv.Mavlink;
using Asv.Mavlink.V2.AsvGbs;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Gbs;

public class BaseStationModeGbsRttViewModel : GbsRttItem
{
    public BaseStationModeGbsRttViewModel()
    {
        BaseStationMode = "Idle";
    }

    public BaseStationModeGbsRttViewModel(IGbsClientDevice baseStation)  : base(baseStation, GenerateUri(baseStation,"basestationmode"))
    {
        Order = 2;

        BaseStation.Gbs.CustomMode
            .Subscribe(_ => BaseStationMode = _.ToString().Replace(nameof(AsvGbsCustomMode),""))
            .DisposeItWith(Disposable);
    }
    
    [Reactive]
    public string BaseStationMode { get; set; } = RS.GbsRttItem_ValueNotAvailable;
}