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

    public ObservationGbsRttViewModel(IGbsDevice gbs,ILocalizationService loc) : base(gbs, GenerateUri(gbs,"observation"))
    {
        Order = 1;
        
        Gbs.DeviceClient.ObservationSec
            .Subscribe(_ => Observation = loc.RelativeTime.ConvertToString(TimeSpan.FromSeconds(_)))
            .DisposeItWith(Disposable);
    }
    
    [Reactive]
    public string Observation { get; set; } = RS.GbsRttItem_ValueNotAvailable;
}