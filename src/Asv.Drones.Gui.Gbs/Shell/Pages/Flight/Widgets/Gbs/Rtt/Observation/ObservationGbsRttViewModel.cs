using Asv.Common;
using Asv.Drones.Gui.Core;
using ReactiveUI.Fody.Helpers;

namespace Asv.Drones.Gui.Gbs;

public class ObservationGbsRttViewModel : GbsRttItem
{
    public ObservationGbsRttViewModel()
    {
        Observation = "30%";
    }

    public ObservationGbsRttViewModel(IGbsDevice gbs) : base(gbs, GenerateUri(gbs,"observation"))
    {
        Order = 1;
        
        Gbs.MavlinkClient.Gbs.Status
            .Subscribe(_ => Observation = _.Observation.ToString())
            .DisposeItWith(Disposable);
    }
    
    [Reactive]
    public string Observation { get; set; } = RS.GbsRttItem_ValueNotAvailable;
}